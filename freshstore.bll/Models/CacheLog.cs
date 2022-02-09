using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace freshstore.bll.Models
{
    public class CacheLog
    {
        [Key]
        public long Id { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public string Type { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        public string Key { get; set; }
        public string? Payload { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
