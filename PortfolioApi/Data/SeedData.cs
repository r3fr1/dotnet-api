using System.Text.Json;
using PortfolioApi.Models;

namespace PortfolioApi.Data
{
    public static class SeedData
    {
        public static void Initialize(ApplicationDbContext ctx, string seedJsonPath)
        {
            if (ctx.Assets.Any()) return; // já populado

            if (File.Exists(seedJsonPath))
            {
                var text = File.ReadAllText(seedJsonPath);
                try
                {
                    using var doc = JsonDocument.Parse(text);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("assets", out var assetsElem))
                    {
                        foreach (var assetEl in assetsElem.EnumerateArray())
                        {
                            var asset = new Asset
                            {
                                Symbol = assetEl.GetProperty("symbol").GetString() ?? "",
                                Name = assetEl.GetProperty("name").GetString() ?? "",
                                Type = assetEl.GetProperty("type").GetString() ?? "",
                                Sector = assetEl.GetProperty("sector").GetString() ?? "",
                                CurrentPrice = assetEl.GetProperty("currentPrice").GetDecimal()
                            };
                            ctx.Assets.Add(asset);
                            ctx.SaveChanges();

                            // optionally price history array
                            if (assetEl.TryGetProperty("priceHistory", out var phArr))
                            {
                                foreach (var ph in phArr.EnumerateArray())
                                {
                                    ctx.PriceHistories.Add(new PriceHistory
                                    {
                                        AssetId = asset.Id,
                                        Date = ph.GetProperty("date").GetDateTime(),
                                        Price = ph.GetProperty("price").GetDecimal()
                                    });
                                }
                                ctx.SaveChanges();
                            }
                        }
                    }

                    if (root.TryGetProperty("portfolios", out var pfArr))
                    {
                        foreach (var pfEl in pfArr.EnumerateArray())
                        {
                            var pf = new Portfolio
                            {
                                Name = pfEl.GetProperty("name").GetString() ?? "",
                                UserId = pfEl.GetProperty("userId").GetString() ?? "",
                                TotalInvestment = pfEl.GetProperty("totalInvestment").GetDecimal()
                            };
                            ctx.Portfolios.Add(pf);
                            ctx.SaveChanges();

                            if (pfEl.TryGetProperty("positions", out var posArr))
                            {
                                foreach (var pos in posArr.EnumerateArray())
                                {
                                    var symbol = pos.GetProperty("symbol").GetString() ?? "";
                                    var asset = ctx.Assets.FirstOrDefault(a => a.Symbol == symbol);
                                    if (asset != null)
                                    {
                                        ctx.Positions.Add(new Position
                                        {
                                            PortfolioId = pf.Id,
                                            AssetId = asset.Id,
                                            Quantity = pos.GetProperty("quantity").GetDecimal(),
                                            AveragePrice = pos.GetProperty("averagePrice").GetDecimal()
                                        });
                                    }
                                }
                                ctx.SaveChanges();
                            }
                        }
                    }
                }
                catch
                {
                    // se parsing falhar, criamos seed default abaixo
                    SeedDefault(ctx);
                }
            }
            else
            {
                SeedDefault(ctx);
            }
        }

        private static void SeedDefault(ApplicationDbContext ctx)
        {
            var a1 = new Models.Asset { Symbol = "PETR4", Name = "Petrobras PN", Type = "Stock", Sector = "Energy", CurrentPrice = 35.50m };
            var a2 = new Models.Asset { Symbol = "VALE3", Name = "Vale ON", Type = "Stock", Sector = "Mining", CurrentPrice = 65.20m };
            ctx.Assets.AddRange(a1, a2);
            ctx.SaveChanges();

            // small price history
            ctx.PriceHistories.AddRange(
                new Models.PriceHistory { AssetId = a1.Id, Date = DateTime.UtcNow.AddDays(-2), Price = 34.10m },
                new Models.PriceHistory { AssetId = a1.Id, Date = DateTime.UtcNow.AddDays(-1), Price = 35.00m },
                new Models.PriceHistory { AssetId = a1.Id, Date = DateTime.UtcNow, Price = 35.50m },
                new Models.PriceHistory { AssetId = a2.Id, Date = DateTime.UtcNow.AddDays(-2), Price = 64.0m },
                new Models.PriceHistory { AssetId = a2.Id, Date = DateTime.UtcNow.AddDays(-1), Price = 64.8m },
                new Models.PriceHistory { AssetId = a2.Id, Date = DateTime.UtcNow, Price = 65.2m }
            );
            ctx.SaveChanges();

            var pf = new Models.Portfolio { Name = "Portfólio Conservador", UserId = "user-001", TotalInvestment = 100000m };
            ctx.Portfolios.Add(pf);
            ctx.SaveChanges();

            ctx.Positions.Add(new Models.Position { PortfolioId = pf.Id, AssetId = a1.Id, Quantity = 1000m, AveragePrice = 30.0m });
            ctx.Positions.Add(new Models.Position { PortfolioId = pf.Id, AssetId = a2.Id, Quantity = 500m, AveragePrice = 60.0m });
            ctx.SaveChanges();
        }
    }
}
