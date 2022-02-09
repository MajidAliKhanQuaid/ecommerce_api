using freshstore.bll.Dtos;
using System.ComponentModel.DataAnnotations;

namespace freshstore.Requests.Basket
{
    public class UpdateBasketItemRequest
    {
        public long ProductId { get; set; }
        [Range(0.000001, double.MaxValue, ErrorMessage = "Please enter a value bigger than 0")]
        public decimal UnitPrice { get; set; }
        [Range(1, long.MaxValue, ErrorMessage = "Please enter a value bigger than {1}")]
        public long Quantity { get; set; } = 0;
    }
}
