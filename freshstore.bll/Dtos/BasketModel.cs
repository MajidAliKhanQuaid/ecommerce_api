using freshstore.bll.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace freshstore.bll.Dtos
{
    public class BasketModel
    {
        public decimal Total { get; set; }
        public decimal Discount { get; set; }
        public int ItemsCount { get; set; }
        public IEnumerable<BasketItemModel> BasketItems { get; set; }
    }
}
