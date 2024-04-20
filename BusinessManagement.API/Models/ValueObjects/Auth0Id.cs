namespace App.Models.ValueObjects
{
    public record Auth0Id
    {
        private Auth0Id() { }
        public Auth0Id(string auth0UserId)
        {
            // Full Auth0 id's will come in the following format: IdP|1234567...
            string[] parts = auth0UserId.Split('|');

            string provider = parts[0];
            string id = parts[1];

            if (string.IsNullOrWhiteSpace(provider))
                throw new ArgumentException("Auth provider is invalid", nameof(auth0UserId));

            if (string.IsNullOrWhiteSpace(id) || id.Length != 24)
                throw new ArgumentException("Auth0 id is invalid", nameof(auth0UserId));

            Auth0UserId = auth0UserId;
        }

        /// <summary>
        /// Return only the auth provider.
        /// </summary>
        /// <returns>Provider name. Example: Auth0</returns>
        public string GetAuth0Provider()
        {
            string[] parts = Auth0UserId.Split('|');

            return parts[0];
        }

        public string Auth0UserId { get; private set; }
    }
}