using Microsoft.AspNetCore.Mvc;
using PortfolioApi.Services;
using PortfolioApi.Constants;

namespace PortfolioApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _svc;
        public AnalyticsController(IAnalyticsService svc) { _svc = svc; }

        [HttpGet("portfolios/{id:int}/performance")]
        public async Task<IActionResult> Performance(int id)
        {
            var res = await _svc.GetPerformanceAsync(id);
            return Ok(res);
        }

        [HttpGet("portfolios/{id:int}/risk-analysis")]
        public async Task<IActionResult> RiskAnalysis(int id, [FromQuery] decimal selic = FinancialConstants.SelicValue)
        {
            var res = await _svc.GetRiskAnalysisAsync(id, selic);
            return Ok(res);
        }

        [HttpPost("portfolios/{id:int}/rebalancing")]
        public async Task<IActionResult> Rebalancing(int id, [FromBody] Dictionary<string, decimal> body)
        {
            var res = await _svc.SuggestRebalancingAsync(id, body);
            return Ok(res);
        }
    }
}
