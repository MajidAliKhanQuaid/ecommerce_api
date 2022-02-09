using System.ComponentModel.DataAnnotations;

namespace freshstore.Requests.Order
{
    public class PlaceDirectOrderRequest
    {
        [Required]
        public List<PlaceDirectOrderItemRequest> Items { get; set; }
    }

    public class PlaceDirectOrderItemRequest
    {
        public long ProductId { get; set; }
        public decimal UnitPrice { get; set; }
        public long Quantity { get; set; }
    }
}
