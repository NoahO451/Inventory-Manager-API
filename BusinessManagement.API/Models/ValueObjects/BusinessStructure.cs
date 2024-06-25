namespace App.Models.ValueObjects
{
    public record BusinessStructure
    {
        public BusinessStructure() { }

        public BusinessStructure(int businessStructureTypeId, string countryCode)
        {
            if (countryCode.Length != 2)
                throw new ArgumentException("Invald Alpha 2 country code", nameof(countryCode));

            BusinessStructureTypeId = businessStructureTypeId;
            CountryCode = countryCode.Trim();
        }
        public int BusinessStructureTypeId { get; private set; }
        public string CountryCode { get; private set; }
    }
}