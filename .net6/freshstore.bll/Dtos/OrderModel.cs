using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace freshstore.bll.Dtos
{
    public class OrderModel
    {
        public long Id { get; set; }
        public DateTime PlacedOn { get; set; }
        public ICollection<OrderItemModel> OrderItems { get; set; }
    }
}
