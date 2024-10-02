﻿using ASP_P15.Data;
using ASP_P15.Models;
using ASP_P15.Models.Home;
using ASP_P15.Models.Product;
using ASP_P15.Models.Shop;
using ASP_P15.Services.FileName;
using ASP_P15.Services.Hash;
using ASP_P15.Services.Kdf;
using ASP_P15.Services.OTP;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ASP_P15.Controllers
{
    public class HomeController : Controller
    {
        // приклад інжекції - _logger
        private readonly ILogger<HomeController> _logger;
        // інжектуємо наш (хеш-) сервіс
        private readonly IHashService _hashService;
        private readonly DataContext _dataContext;
        private readonly IKdfService _kdfService;
        private readonly IOtpService _otpService;
        private readonly IFileNameService _fileNameService;

        private String fileErrorKey = "file-error";
        private String fileNameKey  = "file-name";

        private String productFileErrorKey = "file-error";
        private String productFileNameKey = "file-name";

        public HomeController(ILogger<HomeController> logger, IHashService hashService, IFileNameService fileNameService, IOtpService otpService, DataContext dataContext, IKdfService kdfService)
        {
            _logger = logger;
            _hashService = hashService;
            _dataContext = dataContext;
            _kdfService = kdfService;
            _otpService = otpService;
            _fileNameService = fileNameService;
            /* Інжекція через конструктор - найбільш рекомендований варіант. 
Контейнер служб (інжектор) аналізує параметри конструктора і сам 
підставляє до нього необхідні об'єкти (інстанси) служб
*/
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Shop()
        {
            return RedirectToAction("Index", "Shop");
        }

        public IActionResult Profile()
        {
            if (HttpContext.User.Identity?.IsAuthenticated ?? false)
            {
                String sid = HttpContext.User.Claims.First(c => c.Type == ClaimTypes.Sid).Value;
                return View(new ProfilePageModel()
                {
                    User = _dataContext
                    .Users
                    .Include(u => u.Feedbacks)
                    .ThenInclude(f => f.Product)
                    .First(u => u.Id.ToString() == sid)
                });
            }
            return RedirectToAction(nameof(this.Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }
        
        public IActionResult Intro()
        {
            return View();
        }
        
        public IActionResult Url()
        {
            return View();
        }
        
        public IActionResult Razor()
        {
            return View();
        }

        public IActionResult Ioc()
        {
            ViewData["hash"] = _hashService.Digest("123");
            ViewData["hashCode"] = _hashService.GetHashCode();
            ViewData["password"] = _otpService.GeneratePassword();
            ViewData["fileName"] = _fileNameService.GenerateFileName(10);
            return View();
        }

        public IActionResult SignUp()
        {
            SignUpPageModel model = new();

            // На початку перевіряємо чи є збережена сесія 
            if(HttpContext.Session.Keys.Contains("signup-data"))
            {
                // є дані - це редирект, обробляємо дані
                var formModel = JsonSerializer.Deserialize<SignUpFormModel>(
                    HttpContext.Session.GetString("signup-data")!)!;

                model.FormModel = formModel;
                model.ValidationErrors = _Validate(formModel);

                if(model.ValidationErrors.Where(p => p.Value != null).Count() == 0)
                {
                    // немає помилок валідації - реєструємо у БД
                    String salt = _hashService.Digest(Guid.NewGuid().ToString())[..20];
                    _dataContext.Users.Add(new()
                    {
                        Id = Guid.NewGuid(),
                        Name = formModel.UserName,
                        Email = formModel.UserEmail,
                        Salt = salt,
                        Dk = _kdfService.DerivedKey(formModel.UserPassword, salt),
                        Registered = DateTime.Now,
                        Avatar = HttpContext.Session.GetString(fileNameKey),
                        Role = "Guest"
                    });
                    _dataContext.SaveChanges();
                }


                ViewData["data"] = $"email: {formModel.UserEmail}, name: {formModel.UserName}";

                // Ім'я завантаженого файлу (аватарки) також зберігається у сесії
                if (HttpContext.Session.Keys.Contains(fileNameKey))
                {
                    ViewData["avatar"] = HttpContext.Session.GetString(fileNameKey);
                    HttpContext.Session.Remove(fileNameKey);
                }

                // Видаляємо дані з сесії, щоб уникнути повторного оброблення
                HttpContext.Session.Remove("signup-data");
            }
            return View(model);
        }

        public IActionResult Demo([FromQuery(Name="user-email")] String userEmail, [FromQuery(Name = "user-name")] String userName)
        {
            /* Прийом даних від форми, варіант 1: через параметри action 
             * Зв'язування автоматично відбувається за збігом імен
             * <input name="userName"/> ------ Demo(String userName)
             * якщо в HTML використовуються імена, які неможливі у C#
             * (user-name), то додається атрибут [From...] із зазначенням імені
             * перед потрібним параметром
             * 
             * Варіант 1 використовується коли кількість параметрів невелика (1-2)
             * Більш рекомендований спосіб - використання моделей
             */
            ViewData["data"] = $"email: {userEmail}, name: {userName}";
            return View();
        }

        public IActionResult RegUser(SignUpFormModel formModel)
        {
            HttpContext.Session.SetString("signup-data", 
                JsonSerializer.Serialize(formModel));
            
            if (formModel.UserAvatar != null)
            {
                // 1. Відокремити розширення файлу
                int dotPosition = formModel.UserAvatar.FileName.IndexOf('.');
                if (dotPosition == -1)  // немає розширення файл
                {
                    HttpContext.Session.SetString(fileErrorKey, 
                        "Файли без розширення не приймаються");
                }
                else
                {
                    String ext = formModel.UserAvatar.FileName[dotPosition..];
                    // 2. Перевірити розширення на перелік дозволених
                    if (ext == ".jpg" || ext == ".png" || ext == ".bmp")
                    {
                        // 3. Сформувати ім'я файлу, переконатись, що не перекривається наявний файл
                        String filename;
                        String path = "./Uploads/User/"; //  "./wwwroot/img/upload/";
                        do
                        {
                            filename = Guid.NewGuid().ToString() + ext;
                        } while (System.IO.File.Exists(path + filename));
                        // 4. Зберегти файл, зберегти у БД ім'я файлу.
                        using Stream writer = new StreamWriter(path + filename).BaseStream;
                        formModel.UserAvatar.CopyTo(writer);
                        HttpContext.Session.SetString(fileNameKey, filename);
                    }
                    else
                    {
                        HttpContext.Session.SetString(fileErrorKey,
                        "Файл має недозволене розширення");
                    }
                }
                
                
            }
            
            return RedirectToAction(nameof(SignUp));

            // ViewData["data"] = $"email: {formModel.UserEmail}, name: {formModel.UserName}";
            // return View("Demo");
            /* Проблема: якщо сторінка побудована через передачу форми, то
             * її оновлення у браузері
             * а) видає повідомлення, на яке ми не впливаємо
             * б) повторно передає дані форми, що може призвести до 
             *     дублювання даних у БД, файлів, тощо
             * Рішення: "скидання даних" - переадресація відповіді із 
             *  запам'ятовуванням даних
             *  
             *  Client(Browser)                    Server(ASP)
             *  [form]--------- POST RegUser --------->  [form]---Session
             *  <-------------- 302 SignUp -----------             |
             *  --------------- GET SignUp ----------->            |
             *   <-----------------HTML----------------------- оброблення
             *   
             * Включення та налаштування сесій - див. https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state  
             */
        }

        public IActionResult Product()
        {
            ProductPageModel model = new ProductPageModel();
            if (HttpContext.Session.Keys.Contains("product-data"))
            {
                var formModel = JsonSerializer.Deserialize<ProductFormModel>(HttpContext.Session.GetString("product-data")!)!;
                model.FormModel = formModel;
                model.ValidationErrors = _ValidateProduct(formModel);

                ViewData["productData"] = $"name: {formModel.Name}, description: {formModel.Description}, price: {formModel.Price}, amount: {formModel.Amount}";

                if (HttpContext.Session.Keys.Contains(productFileNameKey))
                {
                    ViewData["picture"] = HttpContext.Session.GetString(productFileNameKey);
                    HttpContext.Session.Remove(productFileNameKey);
                }

                HttpContext.Session.Remove("product-data");
            }
            return View(model);
        }
        public IActionResult AddProduct(ProductFormModel formModel)
        {
            HttpContext.Session.SetString("product-data", JsonSerializer.Serialize(formModel));

            if (formModel.Picture != null)
            {
                int dotPosition = formModel.Picture.FileName.IndexOf(".");
                if (dotPosition == -1) HttpContext.Session.SetString(productFileErrorKey, "Файл не вибрано");
                else
                {
                    String ext = formModel.Picture.FileName[dotPosition..];
                    String[] extentions = [".jpg", ".png", ".bmp"];
                    if (!extentions.Contains(ext)) HttpContext.Session.SetString(productFileErrorKey, "Файл не вибрано 2");
                    else
                    {
                        String filename;
                        String path = "./Uploads/Product/";
                        do
                        {
                            filename = new FileNameService().GenerateFileName(16) + ext;
                        } while (System.IO.File.Exists(path + filename));

                        using Stream writer = new StreamWriter(path + filename).BaseStream;
                        formModel.Picture.CopyTo(writer);

                        HttpContext.Session.SetString(productFileNameKey, filename);
                    }
                }
            }

            return RedirectToAction(nameof(Product));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Download([FromRoute] String id)
        {
            id = id.Replace('_', '/');
            String filename = $"./Uploads/{id}";
            if (System.IO.File.Exists(filename))
            {
                var stream = new StreamReader(filename).BaseStream;
                return File(stream, "image/png");
            }
            return NotFound();
        }

        public IActionResult DownloadProduct([FromRoute] String id)
        {
            String filename = $"./Uploads/Product/{id}";
            if (System.IO.File.Exists(filename))
            {
                var stream = new StreamReader(filename).BaseStream;
                return File(stream, "image/png");
            }
            return NotFound();
        }

        private Dictionary<String, String?> _Validate(SignUpFormModel model)
        {
            /* Валідація - перевірка даних на відповідність певним шаблонам/правилам
             * Результат валідації - {
             *   "UserEmail": null,          null - як ознака успішної валідації
             *   "UserName": "Too short"     значення - повідомлення про помилку
             * }
             */
            Dictionary<String, String?> res = new();

            var emailRegex = new Regex(@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$");
            res[nameof(model.UserEmail)] =
                String.IsNullOrEmpty(model.UserEmail)
                ? "Не допускається порожнє поле"
                : emailRegex.IsMatch(model.UserEmail)
                    ? null
                    : "Введіть коректну адресу";
            
            var nameRegex = new Regex(@"^\w{2,}(\s+\w{2,})*$");
            res[nameof(model.UserName)] =
                String.IsNullOrEmpty(model.UserName)
                ? "Не допускається порожнє поле"
                : nameRegex.IsMatch(model.UserName)
                    ? null
                    : "Введіть коректне ім'я";

            if (String.IsNullOrEmpty(model.UserPassword))
            {
                res[nameof(model.UserPassword)] = "Не допускається порожнє поле";
            }
            else if(model.UserPassword.Length < 3)
            {
                res[nameof(model.UserPassword)] = "Пароль має бути не коротшим за 8 символів";
            }
            else 
            {
                List<String> parts = [];
                if (!Regex.IsMatch(model.UserPassword, @"\d"))
                {
                    parts.Add(" одну цифру");
                }
                if (!Regex.IsMatch(model.UserPassword, @"\D"))
                {
                    parts.Add(" одну літеру");
                }
                if (!Regex.IsMatch(model.UserPassword, @"\W"))
                {
                    parts.Add(" один спецсимвол");
                }
                if (parts.Count > 0)
                {
                    res[nameof(model.UserPassword)] = 
                        "Пароль повинен містити щонайменше" + String.Join(',', parts);
                }
                else
                {
                    res[nameof(model.UserPassword)] = null;
                }                
            }


            res[nameof(model.UserRepeat)] = model.UserPassword == model.UserRepeat
                ? null
                : "Паролі не збігаються";


            res[nameof(model.IsAgree)] = model.IsAgree 
                ? null 
                : "Необхідно прийняти правила сайту";

            // Результати перевірки файлу збережені у сесії
            if (HttpContext.Session.Keys.Contains(fileErrorKey))
            {
                res[nameof(model.UserAvatar)] = 
                    HttpContext.Session.GetString(fileErrorKey);
                HttpContext.Session.Remove(fileErrorKey);
            }

            return res;
        }

        private Dictionary<String, String?> _ValidateProduct(ProductFormModel model)
        {
            Dictionary<String, String?> res = new();

            var nameRegex = new Regex(@"^\w{2,}(\s+\w{2,})*$");
            res[nameof(model.Name)] = String.IsNullOrEmpty(model.Name) ? "Поле не повинно бути порожнім" : null;

            res[nameof(model.Description)] = String.IsNullOrEmpty(model.Description) ? "Поле не повинно бути порожнім" : null;

            res[nameof(model.Price)] = model.Price <= 0 ? "Ціна повинна бути більшою за 0" : null;

            res[nameof(model.Amount)] = model.Amount <= 0 ? "Кількість повинна бути більшою за 0" : null;

            return res;
        }
    }
}
