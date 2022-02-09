using freshstore.bll.Dtos;
using System.ComponentModel.DataAnnotations;

namespace freshstore.Responses.Basket
{
    public class UpdateBasketItemResponse
    {
        public long BasketId { get; set; }
        //
        public long ProductId { get; set; }
        public virtual ProductModel Product { get; set; }
        [Range(0.000001, double.MaxValue, ErrorMessage = "Please enter a value bigger than 0")]
        public decimal UnitPrice { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Please enter a value bigger than {1}")]
        public long Quantity { get; set; }
    }
}
