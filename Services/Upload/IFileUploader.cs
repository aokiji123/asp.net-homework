namespace ASP_P15.Services.Upload
{
    public interface IFileUploader
    {
        String UploadFile(IFormFile file, String? path);
    }
}
