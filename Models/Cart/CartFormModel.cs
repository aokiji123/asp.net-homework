namespace ASP_P15.Models.Cart
{
    public class CartFormModel
    {
        public Guid ProductId { get; set; }
        public Guid UserId { get; set; }
        public int Count { get; set; }
    }
}