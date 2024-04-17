using App.Models.ValueObjects;

namespace App.Models.DTO.Responses
{
    public record NewUserSignupResponse
    {
        public Guid UserUuid { get; init; }
        public string FirstName { get; init; }
        public string LastName { get; init; }
        public string Email { get; init; }
        public bool IsPremiumMember { get; init; }
    }
}
