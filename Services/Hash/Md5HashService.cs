namespace ASP_P15.Services.Hash
{
    public class Md5HashService : IHashService
    {
        public String Digest(String input)
        {
            return Convert.ToHexString(
                System.Security.Cryptography.MD5.HashData(
                    System.Text.Encoding.UTF8.GetBytes(input)
                )
            );
        }
    }
}
