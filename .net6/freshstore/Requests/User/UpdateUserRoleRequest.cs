using System.ComponentModel.DataAnnotations;

namespace freshstore.Requests.User
{
    public class UpdateUserRoleRequest
    {
        [Required]
        [Range(1, long.MaxValue, ErrorMessage = "Enter valid role Id")]
        public long RoleId { get; set; }
    }
}
