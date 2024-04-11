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

            Auth0UserId = id;
            AuthProvider = provider;
        }

        /// <summary>
        /// // Includes the full IdP|1234567...
        /// </summary>
        public string Auth0UserId { get; private set; }
        /// <summary>
        /// Only includes the digits after the IdP
        /// </summary>
        public string AuthProvider { get; private set; }
    }
}