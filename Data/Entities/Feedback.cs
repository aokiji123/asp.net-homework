namespace ASP_P15.Data.Entities
{
    public class Feedback
    {
        public Guid   Id        { get; set; }
        public Guid   ProductId { get; set; }
        public Guid   UserId    { get; set; }
        public String Text      { get; set; }
        public int    Rate      { get; set; } = 5;   // stars
        public long Timestamp { get; set; }

        public DateTime? DeleteDt { get; set; }

        public Product? Product { get; set; }
        public User? User { get; set; }

    }
}
