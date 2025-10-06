using System.ComponentModel.DataAnnotations;

namespace PortfolioApi.Models
{
    public class Asset
    {
        private List<PriceHistory> priceHistory = new();

        [Key]
        public int Id { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Sector { get; set; } = string.Empty;
        public decimal CurrentPrice { get; set; }
        public List<PriceHistory> PriceHistory { get => priceHistory; set => priceHistory = value; }
    }
}
