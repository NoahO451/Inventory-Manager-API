using App.Models.ValueObjects;

namespace App.Models.DTO.Responses
{
    public record UpdateUserDemographicsResponse
    {
        public string FirstName { get; init; }
        public string LastName { get; init; }
        public string EmailAddress { get; init; }
    }
}