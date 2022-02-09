using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace freshstore.bll.Models
{
    public class User : BaseKernel
    {
        [Column(TypeName = "nvarchar(200)")]
        public string Name { get; set; }
        [Column(TypeName = "nvarchar(500)")]
        public string Email { get; set; }
        [Column(TypeName = "nvarchar(500)")]
        public string Password { get; set; }
        public Nullable<DateTime> LastAccessedOn { get; set; }
        [Column(TypeName = "nvarchar(200)")]
        public string LastAccessedIp { get; set; }

        // users are confirmed by default
        // confirmation is used for admin roles (like admin will approve the admins)
        public bool IsConfirmed { get; set; }
        public Nullable<int> ConfirmedBy { get; set; }
        public Nullable<DateTime> ConfirmedOn { get; set; }

        public virtual ICollection<Address> Addresses { get; set; }
        public virtual ICollection<Basket> Baskets { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<Role> Roles { get; set; }
        public virtual ICollection<UserLevelPermission> Permissions { get; set; }
    }
}
