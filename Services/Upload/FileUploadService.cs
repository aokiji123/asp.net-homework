
namespace ASP_P15.Services.Upload
{
    public class FileUploadService : IFileUploader
    {
        public String UploadFile(IFormFile file, String? path)
        {
            ArgumentNullException.ThrowIfNull(file, nameof(file));

            // 1. Відокремити розширення файлу
            int dotPosition = file.FileName.IndexOf('.');
            if (dotPosition == -1)  // немає розширення файлу
            {
                throw new ArgumentException(
                    "Файли без розширення не приймаються");
            }
            else
            {
                String ext = file.FileName[dotPosition..];
                // 2. Перевірити розширення на перелік дозволених
                if (ext == ".jpg" || ext == ".png" || ext == ".bmp")
                {
                    // 3. Сформувати ім'я файлу, переконатись, що не перекривається наявний файл
                    String filename;
                    String fullName;
                    path ??= "./Uploads/"; //  "./wwwroot/img/upload/";
                    do
                    {
                        filename = Guid.NewGuid().ToString() + ext;
                        fullName = Path.Combine(path, filename);
                    } while (System.IO.File.Exists(fullName));
                    // 4. Зберегти файл, зберегти у БД ім'я файлу.
                    using Stream writer = new StreamWriter(fullName).BaseStream;
                    file.CopyTo(writer);
                    return filename;
                }
                else
                {
                    throw new ArgumentException(
                        "Файл має недозволене розширення");
                }
            }
        }
    }
}
