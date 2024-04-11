using App.Models.ValueObjects;

namespace App.Models
{
    public class UserData
    {
        public UserData() { }
        public UserData(Guid userUuid, Auth0Id auth0Id, Name name, Email email, UserBusiness? businesses,
            DateTime createdAt, DateTime? lastLogin, bool isPremiumMember, bool isDeleted)
        {

            if (createdAt < DateTime.Now)
                throw new ArgumentException("Created time cannot be in the past", nameof(createdAt));

            UserUuid = userUuid;
            Auth0Id = auth0Id;
            Name = name;
            Email = email;
            Businesses = businesses;
            CreatedAt = createdAt;
            LastLogin = lastLogin;
            IsPremiumMember = isPremiumMember;
            IsDeleted = isDeleted;
        }

        public void SetAuth0Id(string auth0Id)
        {
            if (string.IsNullOrWhiteSpace(auth0Id))
                throw new ArgumentException("Auth0Id is null or white space", nameof(auth0Id));

            Auth0Id = new Auth0Id(auth0Id);
        }

        public void SetName(string firstName, string lastName)
        {
            if (string.IsNullOrEmpty(firstName))
                throw new ArgumentException("User must have first name", nameof(firstName));

            if (string.IsNullOrEmpty(lastName))
                throw new ArgumentException("User must have last name", nameof(lastName));

            Name = new Name(firstName, lastName);
        }

        public void SetEmail(string emailAddress)
        {
            if (string.IsNullOrWhiteSpace(emailAddress))
                throw new ArgumentException("Email is null or white space", nameof(emailAddress));

            Email = new Email(emailAddress);
        }

        public Guid UserUuid { get; private set; }
        public Auth0Id Auth0Id { get; private set; }
        public Name Name { get; private set; }
        public Email Email { get; private set; }
        public UserBusiness? Businesses { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? LastLogin { get; private set; }
        public bool IsPremiumMember { get; private set; }
        public bool IsDeleted { get; private set; }
    }
}