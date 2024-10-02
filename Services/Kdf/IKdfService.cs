namespace ASP_P15.Services.Kdf
{
    public interface IKdfService
    {
        String DerivedKey(String password, String salt);
    }
}
