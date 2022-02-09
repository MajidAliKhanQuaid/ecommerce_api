using freshstore.bll.Enums;

namespace freshstore.Responses.Address
{
    public class GetAddressResponse
    {
        public long Id { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Phone { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public AddressType Type { get; set; }
    }
}
