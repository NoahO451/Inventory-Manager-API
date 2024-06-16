namespace App.Models.DTO.Responses
{
    public record CreateNewBusinessRequest
    {
        public Guid BusinessOwnerUuid { get; init; }
        public string BusinessFullname { get; init; }
        public string? BusinessDisplayName { get; init; }
        public int BusinessStructureTypeId { get; init; }
        public string CountryCode { get; init; }
        public string BusinessIndustry { get; init; }
    }
}
