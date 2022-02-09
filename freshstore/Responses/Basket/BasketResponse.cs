using freshstore.bll.Dtos;

namespace freshstore.Responses.Basket
{
    public class BasketResponse
    {
        public decimal Total { get; set; }
        public decimal Discount { get; set; }
        public long ItemsCount { get; set; }
        public IEnumerable<BasketItemModel> BasketItems { get; set; }
    }
}
