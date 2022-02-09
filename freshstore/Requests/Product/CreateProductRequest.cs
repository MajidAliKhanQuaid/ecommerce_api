using freshstore.bll.Dtos;

namespace freshstore.Requests.Product
{
    public class CreateProductRequest
    {
        public string Name { get; set; }
        public Nullable<long> Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public IEnumerable<CategoryModel> Categories { get; set; }
    }
}
