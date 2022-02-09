using System.ComponentModel.DataAnnotations;

namespace freshstore.Requests.User
{
    public class RegisterUserRequest
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
