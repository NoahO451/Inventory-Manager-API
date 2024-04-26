using NameParser;

namespace App.Models.ValueObjects
{
    public record Name
    {
        private Name() { }
        public Name(string fullName, string nickname)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("FullName name was null or whitespace", nameof(fullName));

            if (string.IsNullOrWhiteSpace(nickname))
                throw new ArgumentException("Nickname was null or whitespace", nameof(nickname));

            FullName = fullName.Trim();
            Nickname = nickname.Trim();

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

        public string FullName { get; private set; }
        public string FirstName { get; private set; } = string.Empty;
        public string LastName { get; private set; } = string.Empty;
        public string Nickname { get; private set; }
    }
}