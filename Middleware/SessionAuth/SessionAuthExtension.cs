namespace ASP_P15.Middleware.SessionAuth
{
    public static class SessionAuthExtension
    {
        public static IApplicationBuilder UseSessionAuth(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SessionAuthMiddleware>();
        }
    }
}
