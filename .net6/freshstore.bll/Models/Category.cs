using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace freshstore.bll.Models
{
    public class Category : BaseKernel
    {
        [Column(TypeName = "nvarchar(200)")]
        public string Name { get; set; }

        public virtual ICollection<Product> Products { get; set; }
    }
}
