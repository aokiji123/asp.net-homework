using ASP_P15.Data.Entities;

namespace ASP_P15.Models.Shop
{
    public class ShopProductPageModel
    {
        public Data.Entities.Product Product { get; set; } = null!;
        public ProductGroup ProductGroup { get; set; } = null!;
    }
}
