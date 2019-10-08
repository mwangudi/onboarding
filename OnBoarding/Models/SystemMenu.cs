namespace OnBoarding.Models
{
    using System;

    public partial class SystemMenu
    {
        public int Id { get; set; }
        public string nameOption { get; set; }
        public string controller { get; set; }
        public string action { get; set; }
        public string imageClass { get; set; }
        public bool status { get; set; }
        public bool isHomePage { get; set; }
        public bool isParent { get; set; }
        public int parentId { get; set; }
        public string expanded { get; set; }
        public DateTime dateCreated { get; set; }
        public string createdBy { get; set; }
    }
}                                                                                                                                                                                                                          