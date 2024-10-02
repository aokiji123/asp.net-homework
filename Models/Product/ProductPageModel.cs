namespace ASP_P15.Models.Product
{
    public class ProductPageModel
    {
        public ProductFormModel? FormModel { get; set; }
        public Dictionary<String, String?>? ValidationErrors { get; set; }
    }
}
