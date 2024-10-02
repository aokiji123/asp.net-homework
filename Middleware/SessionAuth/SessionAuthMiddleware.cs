using ASP_P15.Data;
using System.Globalization;
using System.Security.Claims;

namespace ASP_P15.Middleware.SessionAuth
{
    public class SessionAuthMiddleware
    {
        // При побудові проєкту визначається послідовність запуску
        // всіх Middleware і кожен з них у конструктор приймає посилання
        // на наступний - _next, задача - зберегти його
        private readonly RequestDelegate _next;

        public SessionAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        // Робота Middleware - кожен запит буде оброблятись цим методом
        // Оскільки конструктор вже задіяний, інжекція здійснюється через
        // параметри методу
        public async Task InvokeAsync(HttpContext context, DataContext dataContext)
        {
            // перевіряємо чи є у сесії збережений токен
            if(context.Session.Keys.Contains("token"))
            {
                // вилучаємо ID токена з сесійного сховища
                String tokenId = context.Session.GetString("token")!;
                // переводимо до GUID
                Guid id = Guid.Parse(tokenId);
                // шукаємо у БД і перевіряємо чи знайдений
                if( dataContext.Tokens.Find(id) is Data.Entities.Token token)
                {
                    // перевіряємо придатність (термін дії)
                    if (token.ExpiresAt > DateTime.Now)
                    {
                        // зберігаємо токен у контексті
                        // context.Items.Add("token", token);

                        // через токен знаходимо кому він виданий (користувача)
                        if (dataContext.Users.Find(token.UserId) is Data.Entities.User user)
                        {
                            // Уніфікований підхід - Claims:
                            // "перекладаємо" дані з Entities.User до типових Claims
                            System.Security.Claims.Claim[] claims = [
                                new(ClaimTypes.Email,    user.Email),
                                new(ClaimTypes.Name,     user.Name ),
                                new(ClaimTypes.Sid,      user.Id.ToString()),
                                new(ClaimTypes.UserData, user.Avatar ?? ""),

                                new(ClaimTypes.DateOfBirth, user.Birthdate?.ToShortDateString() ?? "- - -"),
                                new("User::Registered", user.Registered.ToString()),
                                new(ClaimTypes.Role, user.Role)
                            ];
                            // в ASP у HttpContext є властивість User, що є "власником" Claims
                            context.User = new ClaimsPrincipal(     // користувач може пройти
                                new ClaimsIdentity(                 // декілька авторизацій або
                                    claims,                         // одночасно, або одну з них.
                                    nameof(SessionAuthMiddleware)   // Для розрізнення додається
                                )                                   // параметр authenticationType
                            );                                      // (nameof(SessionAuthMiddleware))
                        }
                    }
                }
            }
            // Передача управління наступному Middleware
            await _next(context);
            // Після цього виклику - зворотний хід (вихід Response)
        }
    }
}
