using App.Models.ValueObjects;
using FluentValidation;

namespace App.Models.DTO.Requests
{
    public record UpdateUserDemographicsRequest
    {
        // Uuid cannot be changed but we still need it
        public Guid UserUuid { get; init; }
        public string FirstName { get; init; }
        public string LastName { get; init; }
        public string EmailAddress { get; init; }
    }
}
