using System.ComponentModel.DataAnnotations;

namespace PortfolioApi.Dtos
{
    public class PositionUpdateDto
    {
        [Range(1, double.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
        public decimal Quantity { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "Average price must be a positive number.")]
        public decimal AveragePrice { get; set; }
    }
}
