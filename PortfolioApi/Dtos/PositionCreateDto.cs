using System.ComponentModel.DataAnnotations;

namespace PortfolioApi.Dtos
{
    public class PositionCreateDto
    {
        public int AssetId { get; set; }
        
        [Range(1, double.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
        public int Quantity { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "Average price must be a positive number.")]
        public decimal AveragePrice { get; set; }
    }
}
