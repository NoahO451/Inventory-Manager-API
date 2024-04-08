using App.Models.ValueObjects;

namespace App.Models.DTO.Requests
{
    public class NewUserSignupRequest
    {
        public string Auth0Id { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }
}