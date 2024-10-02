namespace ASP_P15.Data.Entities
{
    public class Product
    {
        public Guid      Id          { get; set; }
        public Guid      GroupId     { get; set; }
        public string    Name        { get; set; }
        public string?   Description { get; set; }
        public string?   Image       { get; set; }
        public double    Price       { get; set; }
        public long      Amount      { get; set; }
        public DateTime? DeleteDt    { get; set; }
        public String?   Slug        { get; set; }

        public ProductGroup Group { get; set; }
        public IEnumerable<Feedback> Feedbacks { get; set; }
    }
}
