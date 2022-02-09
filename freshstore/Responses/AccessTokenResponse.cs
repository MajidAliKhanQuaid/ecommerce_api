namespace freshstore.Responses
{
    public class AccessTokenResponse
    {
        public string AccessToken { get; set; }
        public DateTime AccessTokenExpiresOnUtc { get; set; }
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiresOnUtc { get; set; }
    }
}
