using freshstore.bll.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace freshstore.bll.Models
{
    public class Order : BaseKernel
    {
        public long UserId { get; set; }
        public OrderStatus Status { get; set; }
        public virtual User User { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }
}
