using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace freshstore.bll.Models
{
    public class UserLevelPermission : BaseKernelWithOutUser
    {
        public string Name { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}
