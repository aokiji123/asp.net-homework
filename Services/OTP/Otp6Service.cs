namespace ASP_P15.Services.OTP
{
    public class Otp6Service : IOtpService
    {
        public String GeneratePassword()
        {
            return new Random().Next(100000, 999999).ToString();
        }
    }
}
