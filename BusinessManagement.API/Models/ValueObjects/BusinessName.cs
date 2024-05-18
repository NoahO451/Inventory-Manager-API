namespace App.Models.ValueObjects
{
    public record BusinessName
    {
        private BusinessName() { }

        public BusinessName(string businessFullName, string? businessDisplayName)
        {
            if (string.IsNullOrEmpty(businessFullName))
                throw new ArgumentException("Business full name was null or whitespace", nameof(businessFullName));

            BusinessFullName = businessFullName.Trim();
            BusinessDisplayName = businessDisplayName.Trim();
        }
        public string BusinessFullName { get; private set; }
        public string? BusinessDisplayName { get; private set; }
    }
}
