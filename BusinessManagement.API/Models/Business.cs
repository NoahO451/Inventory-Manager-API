namespace App.Models
{
    public class Business
    {
        public Guid BusinessUuid { get; set; }
        public string BusinessName { get; set; }
        public int BusinessType { get; set; }
        public string BusinessIndustry{ get; set; }
        public bool IsDeleted { get; set; }
    }
}