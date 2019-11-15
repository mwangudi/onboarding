namespace OnBoarding.ViewModels
{
    public class DesignaterUserViewModel
    {
        public int Id { get; set; }
        public int RepresentativeId { get; set; }
        public string Names { get; set; }
        public bool EMarketSignUp { get; set; }
        public string TradingLimit { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string Signature { get; set; }
    }

    public class ResetRepresentativeOTPViewModel
    {
        public int getRepresentativeId { get; set; }
    }

}