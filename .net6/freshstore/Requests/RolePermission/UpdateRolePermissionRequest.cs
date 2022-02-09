using System.ComponentModel.DataAnnotations;

namespace freshstore.Requests.RolePermission
{
    public class UpdateRolePermissionRequest
    {
        [Required]
        [Range(1, long.MaxValue, ErrorMessage = "Enter valid role Id")]
        public long RoleId { get; set; }
        [Required]
        [Range(1, long.MaxValue, ErrorMessage = "Enter valid permission Id")]
        public long RolePermissionId { get; set; }
    }
}
