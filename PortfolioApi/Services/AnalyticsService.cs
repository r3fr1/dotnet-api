using Microsoft.EntityFrameworkCore;
using PortfolioApi.Constants;
using PortfolioApi.Data;
using PortfolioApi.Models;
using System.Globalization;

namespace PortfolioApi.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly ApplicationDbContext _ctx;

        public AnalyticsService(ApplicationDbContext ctx) { _ctx = ctx; }

        public async Task<object> GetPerformanceAsync(int portfolioId)
        {
            var pf = await _ctx.Portfolios
                .Include(p => p.Positions)
                    .ThenInclude(pos => pos.Asset)
                .FirstOrDefaultAsync(p => p.Id == portfolioId);

            if (pf == null) throw new Exception(Messages.PortfolioNotFound);

            decimal currentValue = 0m;
            decimal invested = 0m;
            DateTime? earliest = null;

            foreach (var pos in pf.Positions)
            {
                if (pos.Asset == null)
                {
                    pos.Asset = await _ctx.Assets.FindAsync(pos.AssetId);
                }

                currentValue += pos.Quantity * pos.Asset!.CurrentPrice;
                invested += pos.Quantity * pos.AveragePrice;
            }

            // earliest transaction date fallback: use price history earliest
            foreach (var pos in pf.Positions)
            {
                var ph = await _ctx.PriceHistories.Where(ph => ph.AssetId == pos.AssetId).OrderBy(ph => ph.Date).FirstOrDefaultAsync();
                if (ph != null)
                {
                    if (!earliest.HasValue || ph.Date < earliest.Value) earliest = ph.Date;
                }
            }

            var totalReturnPercent = invested == 0 ? 0m : (currentValue - invested) / invested * 100m;
            double years = 1.0;
            if (earliest.HasValue)
            {
                var days = (DateTime.UtcNow - earliest.Value).TotalDays;
                years = Math.Max(1.0 / 365.0, days / 365.0);
            }

            var totalReturn = (double)(invested == 0 ? 0m : (currentValue / invested) - 1.0m);
            var annualizedReturn = Math.Pow(1.0 + totalReturn, 1.0 / years) - 1.0;

            return new
            {
                invested = invested,
                currentValue = currentValue,
                totalReturnPercent = Math.Round(totalReturnPercent, 4),
                annualizedReturnPercent = Math.Round((decimal)(annualizedReturn * 100.0), 4)
            };
        }

        public async Task<object> GetRiskAnalysisAsync(int portfolioId, decimal selicRate = FinancialConstants.SelicValue)
        {
            var pf = await _ctx.Portfolios
                .Include(p => p.Positions)
                    .ThenInclude(pos => pos.Asset)
                        .ThenInclude(a => a != null ? a.PriceHistory : null!)
                .FirstOrDefaultAsync(p => p.Id == portfolioId);

            if (pf == null) throw new Exception(Messages.PortfolioNotFound);

            // portfolio current value
            decimal currentValue = pf.Positions.Sum(pos => pos.Quantity * (pos.Asset?.CurrentPrice ?? 0m));
            if (currentValue <= 0) return new { message = Messages.EmptyPortfolio };

            // compute per-asset daily returns standard deviation (simple)
            var volPerAsset = new Dictionary<int, double>();
            foreach (var pos in pf.Positions)
            {
                var asset = pos.Asset!;
                var prices = asset.PriceHistory.OrderBy(ph => ph.Date).Select(ph => ph.Price).ToList();
                if (prices.Count < 2) { volPerAsset[asset.Id] = 0.0; continue; }
                var returns = new List<double>();
                for (int i = 1; i < prices.Count; i++)
                {
                    var r = (double)((prices[i] - prices[i - 1]) / prices[i - 1]);
                    returns.Add(r);
                }
                var avg = returns.Average();
                var variance = returns.Select(r => Math.Pow(r - avg, 2)).Average();
                var stddev = Math.Sqrt(variance);
                volPerAsset[asset.Id] = stddev * Math.Sqrt(252); // annualize
            }

            // approximate portfolio volatility as sqrt(sum(w_i^2 * sigma_i^2))
            double portVar = 0.0;
            foreach (var pos in pf.Positions)
            {
                var asset = pos.Asset!;
                var val = (double)(pos.Quantity * asset.CurrentPrice);
                var weight = val / (double)currentValue;
                var sigma = volPerAsset.ContainsKey(asset.Id) ? volPerAsset[asset.Id] : 0.0;
                portVar += weight * weight * sigma * sigma;
            }
            var portVol = Math.Sqrt(portVar);

            // Sharpe Ratio approx: (annualReturn - selic) / volatility
            var perf = await GetPerformanceAsync(portfolioId) as dynamic;
            double annualReturn = 0.0;
            try { annualReturn = (double)((decimal)perf.annualizedReturnPercent / 100m); } catch { annualReturn = 0.0; }

            var sharpe = portVol == 0.0 ? 0.0 : (annualReturn - (double)selicRate) / portVol;

            // concentration: largest asset percent
            var concentrations = pf.Positions.Select(p => new
            {
                symbol = p.Asset!.Symbol,
                value = (double)(p.Quantity * p.Asset.CurrentPrice),
                pct = (double)((p.Quantity * p.Asset.CurrentPrice) / (decimal)currentValue * (decimal)100.0)
            }).OrderByDescending(x => x.pct).ToList();

            return new
            {
                portfolioValue = currentValue,
                volatility = portVol,
                sharpeRatio = sharpe,
                topConcentrations = concentrations.Take(5)
            };
        }

        public async Task<object> SuggestRebalancingAsync(int portfolioId, Dictionary<string, decimal> targetAllocations, decimal transactionFeePercent = FinancialConstants.TransactionFeePercent, decimal minTransactionValue = FinancialConstants.MinTransactionValue)
        {
            var pf = await _ctx.Portfolios
                .Include(p => p.Positions)
                    .ThenInclude(pos => pos.Asset)
                .FirstOrDefaultAsync(p => p.Id == portfolioId);

            if (pf == null) throw new Exception(Messages.PortfolioNotFound);

            decimal currentValue = pf.Positions.Sum(pos => pos.Quantity * (pos.Asset?.CurrentPrice ?? 0m));
            if (currentValue <= 0) return new { message = Messages.EmptyPortfolio };

            // build current weights by symbol
            var currentBySymbol = pf.Positions.ToDictionary(p => p.Asset!.Symbol, p => new { p, value = p.Quantity * p.Asset!.CurrentPrice });

            var suggestions = new List<object>();

            // handle each target allocation symbol
            foreach (var kv in targetAllocations)
            {
                var symbol = kv.Key.ToUpperInvariant();
                var targetPct = kv.Value / 100m;
                var desiredValue = targetPct * currentValue;

                if (currentBySymbol.TryGetValue(symbol, out var cur))
                {
                    var diff = desiredValue - (decimal)cur.value;
                    if (Math.Abs(diff) < minTransactionValue) continue; // ignore
                    var asset = cur.p.Asset!;
                    if (diff > 0)
                    {
                        // buy
                        var effectivePrice = asset.CurrentPrice * (1 + transactionFeePercent);
                        var qty = Math.Floor(diff / effectivePrice);
                        if (qty <= 0) continue;
                        var txValue = qty * asset.CurrentPrice;
                        if (txValue < minTransactionValue) continue;
                        suggestions.Add(new { action = "BUY", symbol, qty, assetPrice = asset.CurrentPrice, estimatedCost = txValue, estimatedFee = Math.Round(txValue * transactionFeePercent, 2) });
                    }
                    else
                    {
                        // sell
                        var effectivePrice = asset.CurrentPrice * (1 - transactionFeePercent);
                        var qty = Math.Floor(Math.Abs(diff) / asset.CurrentPrice);
                        if (qty <= 0) continue;
                        var txValue = qty * asset.CurrentPrice;
                        if (txValue < minTransactionValue) continue;
                        suggestions.Add(new { action = "SELL", symbol, qty, assetPrice = asset.CurrentPrice, estimatedProceeds = txValue, estimatedFee = Math.Round(txValue * transactionFeePercent, 2) });
                    }
                }
                else
                {
                    // asset not in portfolio -> buy
                    var asset = await _ctx.Assets.FirstOrDefaultAsync(a => a.Symbol == symbol);
                    if (asset == null) continue;
                    var desired = desiredValue;
                    if (desired < minTransactionValue) continue;
                    var effectivePrice = asset.CurrentPrice * (1 + transactionFeePercent);
                    var qty = Math.Floor(desired / effectivePrice);
                    if (qty <= 0) continue;
                    var txValue = qty * asset.CurrentPrice;
                    if (txValue < minTransactionValue) continue;
                    suggestions.Add(new { action = "BUY", symbol, qty, assetPrice = asset.CurrentPrice, estimatedCost = txValue, estimatedFee = Math.Round(txValue * transactionFeePercent, 2) });
                }
            }

            // minimize number of transactions: as implemented, we only suggest trades above threshold
            return new
            {
                portfolioValue = currentValue,
                suggestions
            };
        }
    }
}
