using System.ComponentModel.DataAnnotations;

namespace PortfolioApi.Models
{
    public class Portfolio
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public decimal TotalInvestment { get; set; }
        public List<Position> Positions { get; set; } = new();
        public List<Transaction> Transactions { get; set; } = new();
    }
}
