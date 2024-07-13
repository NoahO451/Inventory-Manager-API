using NameParser;

namespace App.Models.ValueObjects
{
    public record CustomerName
    {
        public CustomerName() { }

        public CustomerName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("Full name was null or whitespace", nameof(fullName));

            FullName = fullName.Trim();

            HumanName name = new HumanName(fullName);

            if (name != null && !name.IsUnparsable)
            {
                // The parser trims names so we don't need to do that here
                if (!string.IsNullOrWhiteSpace(name.First))
                    FirstName = name.First;

                if (!string.IsNullOrWhiteSpace(name.Last))
                    LastName = name.Last;
            }
        }

        public string FullName  { get; private set; }
        public string FirstName { get; private set; } = string.Empty;
        public string LastName { get; private set; } = string.Empty;
    }
}
