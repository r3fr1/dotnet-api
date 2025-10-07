using Xunit;
using Moq;
using PortfolioApi.Services;
using PortfolioApi.Repositories;
using PortfolioApi.Data;
using PortfolioApi.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PortfolioApi.Tests.Services
{
    public class PortfolioServiceTests
    {
        private readonly Mock<IPortfolioRepository> _repoMock;
        private readonly ApplicationDbContext _ctx;
        private readonly PortfolioService _service;

        public PortfolioServiceTests()
        {
            _repoMock = new Mock<IPortfolioRepository>();

            // Usando banco em memória
            var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("PortfolioTestDb")
                .Options;
            _ctx = new ApplicationDbContext(options);

            _service = new PortfolioService(_repoMock.Object, _ctx);
        }

        [Fact(DisplayName = "Deve adicionar uma posição corretamente")]
        public async Task DeveAdicionarPosicaoCorretamente()
        {
            // Arrange
            var pf = new Portfolio { Id = 1, Positions = new List<Position>() };
            _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(pf);

            // Act
            await _service.AddPositionAsync(1, 10, 100, 34.0m);

            // Assert
            Assert.Single(_ctx.Positions);
            var pos = await _ctx.Positions.FirstAsync();
            Assert.Equal(1, pos.PortfolioId);
            Assert.Equal(10, pos.AssetId);
            Assert.Equal(100, pos.Quantity);
            Assert.Equal(34.0m, pos.AveragePrice);
        }

        [Fact(DisplayName = "Deve atualizar uma posição corretamente")]
        public async Task DeveAtualizarPosicaoCorretamente()
        {
            // Arrange
            var position = new Position { Id = 1, PortfolioId = 1, AssetId = 10, Quantity = 100, AveragePrice = 10.0m };
            var pf = new Portfolio { Id = 1, Positions = new List<Position> { position } };

            _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(pf);
            _repoMock.Setup(r => r.UpdateAsync(pf)).Returns(Task.CompletedTask);

            // Act
            await _service.UpdatePositionAsync(1, 1, 200, 20.0m);

            // Assert
            Assert.Equal(200, pf.Positions[0].Quantity);
            Assert.Equal(20.0m, pf.Positions[0].AveragePrice);
            _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Portfolio>()), Times.Once);
        }
    }
}
