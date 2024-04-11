using App.Models.ValueObjects;

namespace App.Models.DTO.Responses
{
    public record GetUserResponse
    {
        public Guid UserUuid { get; init; }

        public string Auth0UserId { get; init; }
        public string FirstName { get; init; }
        public string LastName { get; init; }
        public UserBusiness? Businesses { get; init; }
        public DateTime? LastLogin { get; init; }
        public bool IsPremiumMember { get; init; }
        public bool IsDeleted { get; init; }
    }
}
