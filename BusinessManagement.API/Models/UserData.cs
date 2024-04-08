using App.Models.ValueObjects;

namespace App.Models
{
    public class UserData
    {
        public UserData() { }
        public UserData(Guid userUuid, Auth0Id auth0Id, Username username, Name name, Email email, UserRole role, UserBusiness? businesses,
            DateTime createdAt, DateTime? lastLogin, bool isPremiumMember, bool isDeleted)
        {
            UserUuid = userUuid;
            Auth0Id = auth0Id;
            Username = username;
            Name = name;
            Email = email;
            Role = role;
            Businesses = businesses;
            CreatedAt = createdAt;
            LastLogin = lastLogin;
            IsPremiumMember = isPremiumMember;
            IsDeleted = isDeleted;
        }

        public Guid UserUuid { get; private set; }
        public Auth0Id Auth0Id { get; private set; }
        public Username Username { get; private set; }
        public Name Name { get; private set; }
        public Email Email { get; private set; }
        public UserRole Role { get; private set; }
        public UserBusiness? Businesses { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? LastLogin { get; private set; }
        public bool IsPremiumMember { get; private set; }
        public bool IsDeleted { get; private set; }
    }
}
