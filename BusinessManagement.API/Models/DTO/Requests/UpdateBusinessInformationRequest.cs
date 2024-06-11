namespace App.Models.DTO.Requests
{
    public class UpdateBusinessInformationRequest
    {
        public Guid BusinessUuid { get; set; }
        public Guid BusinessOwnerUuid { get; set; }
        public string BusinessFullname { get; set; }
        public string? BusinessDisplayName { get; set; }
        public int BusinessStructureTypeId { get; set; }
        public string CountryCode { get; set; }
        public string BusinessIndustry { get; set; }
    }
}