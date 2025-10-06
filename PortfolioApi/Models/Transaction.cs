using System.ComponentModel.DataAnnotations;

namespace PortfolioApi.Models
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }
        public int PortfolioId { get; set; }
        public Portfolio? Portfolio { get; set; }

        public int AssetId { get; set; }
        public Asset? Asset { get; set; }

        public string Type { get; set; } = "Buy"; // "Buy" or "Sell"
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Fee { get; set; }
        public DateTime Date { get; set; }
    }
}
