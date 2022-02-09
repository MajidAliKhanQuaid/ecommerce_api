using System.ComponentModel.DataAnnotations;

namespace freshstore.Requests.Basket
{
    public class RemoveBasketItemRequest
    {
        [Range(minimum: 1, maximum: long.MaxValue)]
        public long ProductId { get; set; }
    }
}
