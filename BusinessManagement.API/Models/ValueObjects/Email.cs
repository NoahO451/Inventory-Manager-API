namespace App.Models.ValueObjects
{
    public record Email
    {
        private Email() {}
        public Email(string emailAddress)
        {
            if (string.IsNullOrWhiteSpace(emailAddress)) 
                throw new ArgumentNullException("Email address was null or white space", nameof(emailAddress));

            EmailAddress = emailAddress;
        }

        public string EmailAddress { get; set; }
    }
}