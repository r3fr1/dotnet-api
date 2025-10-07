using Xunit;
using PortfolioApi.Services;
using PortfolioApi.Data;
using PortfolioApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace PortfolioApi.Tests.Services
{
    public class AnalyticsServiceTests
    {
        private ApplicationDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase($"AnalyticsTestDb_{Guid.NewGuid()}")
                .Options;

            var ctx = new ApplicationDbContext(options);

            var asset = new Asset { Id = 1, Symbol = "TEST3", CurrentPrice = 15m };
            ctx.Assets.Add(asset);
            ctx.Portfolios.Add(new Portfolio
            {
                Id = 1,
                Name = "Carteira Teste",
                Positions = new List<Position>
                {
                    new Position { Id = 1, Asset = asset, AssetId = 1, Quantity = 10, AveragePrice = 10m }
                }
            });
            ctx.SaveChanges();

            return ctx;
        }

        [Fact(DisplayName = "Deve calcular o desempenho corretamente")]
        public async Task DeveCalcularDesempenhoCorretamente()
        {
            var ctx = CreateContext();
            var service = new AnalyticsService(ctx);

            var result = await service.GetPerformanceAsync(1);
            Assert.NotNull(result);

            var json = System.Text.Json.JsonSerializer.Serialize(result);
            Assert.Contains("totalReturn", json, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("invested", json, StringComparison.OrdinalIgnoreCase);
        }

        [Fact(DisplayName = "Deve retornar risco com Sharpe Ratio calculado")]
        public async Task DeveCalcularRiscoCorretamente()
        {
            var ctx = CreateContext();
            var service = new AnalyticsService(ctx);

            ctx.PriceHistories.AddRange(new[]
            {
                new PriceHistory { AssetId = 1, Date = DateTime.UtcNow.AddDays(-3), Price = 10m },
                new PriceHistory { AssetId = 1, Date = DateTime.UtcNow.AddDays(-2), Price = 12m },
                new PriceHistory { AssetId = 1, Date = DateTime.UtcNow.AddDays(-1), Price = 15m }
            });
            await ctx.SaveChangesAsync();

            var result = await service.GetRiskAnalysisAsync(1);
            Assert.NotNull(result);

            var json = System.Text.Json.JsonSerializer.Serialize(result);
            Assert.Contains("volatility", json, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("sharpe", json, StringComparison.OrdinalIgnoreCase);
        }
    }
}
