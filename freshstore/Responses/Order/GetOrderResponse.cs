using freshstore.bll.Dtos;

namespace freshstore.Responses.Order
{
    public class GetOrderResponse
    {
        public long Id { get; set; }
        public DateTime PlacedOn { get; set; }
        public ICollection<OrderItemModel> OrderItems { get; set; }
    }
}
