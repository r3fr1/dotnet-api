using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PortfolioApi.Models
{
    public class PriceHistory
    {
        [Key]
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Price { get; set; }

        public int AssetId { get; set; }
        public Asset? Asset { get; set; }
    }
}
