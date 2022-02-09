using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace freshstore.bll.Dtos
{
    public class UserModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        //public Nullable<DateTime> LastAccessedOn { get; set; }
        //public string LastAccessedIp { get; set; }
    }
}
