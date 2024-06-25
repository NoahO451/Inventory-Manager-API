namespace App.Models.DTO.Requests
{
    public record UpdateBusinessInformationRequest
    {
        // Business Uuid cannot be changed but we still need it
        public Guid BusinessUuid { get; init; }
        public Guid BusinessOwnerUuid { get; init; }
        public string BusinessFullname { get; init; }
        public string? BusinessDisplayName { get; init; }
        public int BusinessStructureTypeId { get; init; }
        public string CountryCode { get; init; }
        public string BusinessIndustry { get; init; }
    }
}