using freshstore.bll.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace freshstore.bll.Dtos
{
    public class OrderItemModel
    {
        public long ProductId { get; set; }
        public virtual ProductModel Product { get; set; }
        //
        [Column(TypeName = "decimal(18,4)")]
        public decimal UnitPrice { get; set; }
        public long Quantity { get; set; }
        //
        public long OrderId { get; set; }
    }
}
