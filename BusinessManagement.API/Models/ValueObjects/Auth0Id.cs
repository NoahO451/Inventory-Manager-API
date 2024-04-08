namespace App.Models.ValueObjects
{
    public record Auth0Id
    {
        public Auth0Id() { }
        public Auth0Id(string auth0UserId)
        {
            // Auth0 id's will come in the following format: IdP|1234567...
            string[] parts = auth0UserId.Split('|');

            string id = parts[1];

            if (id.Length != 24)
                throw new ArgumentException("Auth0 id is invalid", nameof(auth0UserId));

            Auth0UserId = auth0UserId;
        }

        public string Auth0UserId { get; private set; }
    }
}
