namespace App.Models.ValueObjects
{
    public class Email
    {
        public Email() {}
        public Email(string emailAddress)
        {
            if (string.IsNullOrWhiteSpace(emailAddress)) 
                throw new ArgumentNullException("Email address was null or white space", nameof(emailAddress));

            // We aren't validating emails because the only way to validate an email is to email it
            EmailAddress = emailAddress;
        }

        public string EmailAddress { get; set; }
    }
}
