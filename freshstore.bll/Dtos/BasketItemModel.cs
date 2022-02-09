using freshstore.bll.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace freshstore.bll.Dtos
{
    public class BasketItemModel
    {
        public long BasketId { get; set; }
        //
        public long ProductId { get; set; }
        public virtual ProductModel Product { get; set; }
        [Range(0.000001, double.MaxValue, ErrorMessage = "Please enter a value bigger than 0")]
        public decimal UnitPrice { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Please enter a value bigger than {1}")]
        public long Quantity { get; set; }
    }
}
