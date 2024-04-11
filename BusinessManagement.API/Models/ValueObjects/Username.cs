using System.Text.RegularExpressions;

namespace App.Models.ValueObjects
{
    public record Username
    {
        private Username() {}
        public Username(string username, Guid userUuid)
        {
            if (username.Length < 2 || username.Length > 20)
                throw new ArgumentException("Username must be between 2 and 20 characters long", nameof(username));

            // This regex checks for unicode letters, number, hyphens, and underscores
            string validChars = @"^[\p{L}\p{M}0-9_-]{2,30}$";

            if (!Regex.IsMatch(username, validChars))
                throw new ArgumentException("Username contains one or more invalid characters", nameof(username));

            DisplayName = username;
            UniqueDisplayName = $"{username}|{userUuid}";
        }

        public string DisplayName { get; private set; }
        public string UniqueDisplayName { get; set; }
    }
}