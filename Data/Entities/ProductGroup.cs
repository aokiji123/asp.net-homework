namespace ASP_P15.Data.Entities
{
    public class ProductGroup
    {
        public Guid      Id          { get; set; }
        public string    Name        { get; set; }
        public string    Description { get; set; }
        public string?   Image       { get; set; }
        public DateTime? DeleteDt    { get; set; }
        public String?   Slug        { get; set; }

        public IEnumerable<Product> Products { get; set; }
    }
}
