namespace App.Models
{
    public class UserData
    {
        public int UserId { get; set; }
        public string Auth0Id { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public int Role { get; set; }
        public IReadOnlyCollection<string>? Businesses { get; private set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
        public bool IsPremiumMember { get; set; }
        public bool IsDeleted { get; set; }
    }
}
