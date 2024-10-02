using ASP_P15.Data.Entities;
using System.Text.Json.Serialization;

namespace ASP_P15.Data.Entities
{
    public class CartProduct
    {
        public Guid Id { get; set; }
        public Guid CartId { get; set; }
        public Guid ProductId { get; set; }
        public int Count { get; set; } = 1;

        [JsonIgnore]
        public Cart Cart { get; set; }
        public Product Product { get; set; }
    }
}