using PortfolioApi.Constants;

namespace PortfolioApi.Services
{
    public interface IAnalyticsService
    {
        Task<object> GetPerformanceAsync(int portfolioId);
        Task<object> GetRiskAnalysisAsync(int portfolioId, decimal selicRate = FinancialConstants.SelicValue);
        Task<object> SuggestRebalancingAsync(int portfolioId, Dictionary<string, decimal> targetAllocations, decimal transactionFeePercent = FinancialConstants.TransactionFeePercent, decimal minTransactionValue = FinancialConstants.MinTransactionValue);
    }
}