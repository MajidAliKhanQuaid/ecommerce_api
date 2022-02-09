using freshstore.bll.Dtos;

namespace freshstore.Responses.Product
{
    public class GetProductResponse
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public Nullable<long> Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public IEnumerable<CategoryModel> Categories { get; set; }
    }
}
