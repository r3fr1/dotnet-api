using Microsoft.AspNetCore.Mvc;
using PortfolioApi.Models;
using PortfolioApi.Services;

namespace PortfolioApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssetsController : ControllerBase
    {
        private readonly IAssetService _svc;
        public AssetsController(IAssetService svc) { _svc = svc; }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _svc.GetAllAsync());

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var a = await _svc.GetByIdAsync(id);
            if (a == null) return NotFound();
            return Ok(a);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string symbol)
        {
            var a = await _svc.GetBySymbolAsync(symbol);
            if (a == null) return NotFound();
            return Ok(a);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Asset asset)
        {
            var created = await _svc.CreateAsync(asset);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}/price")]
        public async Task<IActionResult> UpdatePrice(int id, [FromBody] Dictionary<string, decimal> body)
        {
            if (!body.TryGetValue("price", out var price)) return BadRequest();
            await _svc.UpdatePriceAsync(id, price);
            return NoContent();
        }
    }
}
