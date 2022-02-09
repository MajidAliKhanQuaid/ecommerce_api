using System.ComponentModel.DataAnnotations;

namespace freshstore.Requests.UserPermission
{
    public class UpdateUserPermissionRequest
    {
        [Required]
        public string Permission { get; set; }
    }
}
