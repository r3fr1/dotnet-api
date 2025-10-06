using System.ComponentModel.DataAnnotations;

namespace PortfolioApi.Models
{
    public class Position
    {
        [Key]
        public int Id { get; set; }
        public int PortfolioId { get; set; }
        public Portfolio? Portfolio { get; set; }

        public int AssetId { get; set; }
        public Asset? Asset { get; set; }

        public decimal Quantity { get; set; }
        public decimal AveragePrice { get; set; }
    }
}
