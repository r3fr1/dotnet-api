using Microsoft.AspNetCore.Mvc;
using PortfolioApi.Models;
using PortfolioApi.Services;
using PortfolioApi.Dtos;

namespace PortfolioApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PortfoliosController : ControllerBase
    {
        private readonly IPortfolioService _svc;
        public PortfoliosController(IPortfolioService svc) { _svc = svc; }

        [HttpGet]
        public async Task<IActionResult> GetByUser([FromQuery] string userId)
        {
            var list = await _svc.GetByUserIdAsync(userId);
            return Ok(list);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Portfolio pf)
        {
            var created = await _svc.CreateAsync(pf);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var pf = await _svc.GetByIdAsync(id);
            if (pf == null) return NotFound();
            return Ok(pf);
        }

        [HttpPost("{portfolioId}/positions")]
        public async Task<IActionResult> AddPosition(int portfolioId, [FromBody] PositionCreateDto request)
        {
            await _svc.AddPositionAsync(portfolioId, request.AssetId, request.Quantity, request.AveragePrice);
            return NoContent();
        }

        [HttpPut("{id:int}/positions/{positionId:int}")]
        public async Task<IActionResult> UpdatePosition(int id, int positionId, [FromBody] PositionUpdateDto dto)
        {
            await _svc.UpdatePositionAsync(id, positionId, dto.Quantity, dto.AveragePrice);
            return NoContent();
        }

        [HttpDelete("{id:int}/positions/{positionId:int}")]
        public async Task<IActionResult> DeletePosition(int id, int positionId)
        {
            await _svc.RemovePositionAsync(id, positionId);
            return NoContent();
        }
    }
}
