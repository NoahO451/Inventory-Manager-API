namespace App.Models.DTO.Requests
{
    public class NewUserSignupRequest
    {
        public string FullAuth0Id { get; init; }
        public string FullName { get; init; }
        public string Nickname { get; init; }
        public string Email { get; init; }
    }
}