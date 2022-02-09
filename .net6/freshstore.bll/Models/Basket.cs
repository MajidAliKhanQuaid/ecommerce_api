using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace freshstore.bll.Models
{
    public class Basket : BaseKernel
    {
        public long UserId { get; set; }
        public virtual User User { get; set; }

        public virtual ICollection<BasketItem> BasketItems { get; set; }
    }
}
