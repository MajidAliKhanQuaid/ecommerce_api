namespace freshstore.Config
{
    public class JwtOptions
    {
        public string SigningSecret { get; set; }
        public string EncryptionSecret { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
    }
}
