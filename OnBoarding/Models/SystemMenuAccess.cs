namespace OnBoarding.Models
{
    public partial class SystemMenuAccess
    {
        public int Id { get; set; }
        public int menuId { get; set; }
        public string roleId { get; set; }
        public int Status { get; set; }
        public int displayId { get; set; }
    }
}