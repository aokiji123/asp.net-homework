namespace ASP_P15.Data.Entities
{
    public class Token
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
