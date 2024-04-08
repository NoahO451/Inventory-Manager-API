using System.Text.RegularExpressions;

namespace App.Models.ValueObjects
{
    public record Username
    {
        public Username() {}
        public Username(string username)
        {
            if (username.Length > 20)
                throw new ArgumentException("Username must be 20 characters or less", nameof(username));

            string validChars = @"^[a-zA-ZÀ-ÖØ-öø-ÿ0-9_-]+$";

            if (!Regex.IsMatch(username, validChars))
                throw new ArgumentException("Username contains one or more invalid characters", nameof(username));

            DisplayName = username;
        }

        // Can't have the same prop name as the type. So called it DisplayName. 
        public string DisplayName { get; private set; }
    }
}
