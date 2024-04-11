using App.Models.ValueObjects;

namespace App.Models.DTO.Requests
{
    public class NewUserSignupRequest
    {
        public string FullAuth0Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }
}