namespace App.Models.Entities
{
    public class Business
    {
        public int BusinessId { get; set; }
        public string BusinessName { get; set; }
        public string BusinessType { get; set; }
        public bool IsDeleted { get; set; }
    }
}
