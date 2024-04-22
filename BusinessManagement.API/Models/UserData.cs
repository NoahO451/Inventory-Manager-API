using App.Models.ValueObjects;

namespace App.Models
{
    public class UserData
    {
        public UserData() { }

        public UserData(Guid userUuid, Auth0Id auth0Id, Name name, Email email, UserBusiness? businesses,
            DateTime createdAt, DateTime? lastLogin, bool isPremiumMember, bool isDeleted)
        {
            if (userUuid == Guid.Empty)
                throw new ArgumentException("Uuid empty", nameof(userUuid));

            if (isDeleted)
                throw new ArgumentException("User is deleted", nameof(isDeleted));

            if (createdAt < DateTime.Now)
                throw new ArgumentException("Created at time cannot be in the past", nameof(createdAt));

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

        /// <summary>
        /// Takes in the full auth id, identity provider included. Example: auth0|123456
        /// </summary>
        /// <param name="fullAuth0Id"></param>
        public void SetAuth0Id(Auth0Id fullAuth0Id)
        {
            Auth0Id = fullAuth0Id;
        }

        /// <summary>
        /// Used to set the full name, first name, last name, and/or nickname
        /// </summary>
        /// <param name="name"></param>
        public void SetName(Name name)
        {
            Name = name;
        }

        public void SetEmail(Email email)
        {
            Email = email;
        }

        public Guid UserUuid { get; private set; }
        public Auth0Id Auth0Id { get; private set; }
        public Name Name { get; private set; }
        public Email Email { get; private set; }
        public UserBusiness? Businesses { get; set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? LastLogin { get; private set; }
        public bool IsPremiumMember { get; private set; }
        public bool IsDeleted { get; private set; }
    }
}