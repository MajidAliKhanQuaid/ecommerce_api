using freshstore.bll.Enums;
using System.ComponentModel.DataAnnotations;

namespace freshstore.Requests.Address
{
    public class CreateAddressRequest
    {
        [Required]
        [MinLength(length: 3, ErrorMessage = "Please enter valid address")]
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        [Required]
        public string Phone { get; set; }
        [Required]
        public string Country { get; set; }
        [Required]
        public string City { get; set; }
        public string State { get; set; }
        [Required]
        public string ZipCode { get; set; }
        [Required]
        public AddressType Type { get; set; }
    }
}
