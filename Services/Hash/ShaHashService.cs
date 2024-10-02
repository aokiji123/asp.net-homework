namespace ASP_P15.Services.Hash
{
    public class ShaHashService : IHashService
    {
        public String Digest(String input)
        {
            return Convert.ToHexString(
                System.Security.Cryptography.SHA1.HashData(
                    System.Text.Encoding.UTF8.GetBytes(input)
                )
            );
        }
    }
}
