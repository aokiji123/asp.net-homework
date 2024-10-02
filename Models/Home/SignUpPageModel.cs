namespace ASP_P15.Models.Home
{
    public class SignUpPageModel
    {
        public SignUpFormModel? FormModel { get; set; }
        public Dictionary<String,String?>? ValidationErrors { get; set; }
    }
}
