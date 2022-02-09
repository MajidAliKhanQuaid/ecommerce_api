using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace freshstore.bll.Dtos
{
    public class ProductModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public Nullable<long> Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public IEnumerable<CategoryModel> Categories { get; set; }
    }
}
