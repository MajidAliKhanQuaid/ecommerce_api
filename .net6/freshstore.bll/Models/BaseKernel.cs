using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace freshstore.bll.Models
{

    public class BaseKernel
    {
        public long Id { get; set; }
        [Column(TypeName = "nvarchar(200)")]
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        [Column(TypeName = "nvarchar(200)")]
        public string? UpdatedBy { get; set; }
        public Nullable<DateTime> UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class BaseKernelWithOutKeyAndUser
    {
        public bool IsDeleted { get; set; }
    }

    public class BaseKernelWithOutUser
    {
        public long Id { get; set; }
        public bool IsDeleted { get; set; }
    }

}
