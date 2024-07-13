namespace App.Models.ValueObjects
{
    public record Address
    {
        public Address () { }

        public Address(Guid addressUuid, string streetOne, string? streetTwo, string city, string state, 
            string postalCode, string country)
        {
            if (addressUuid == Guid.Empty)
                throw new ArgumentException("Address uuid empty", nameof(addressUuid));

            AddressUuid = addressUuid;
            StreetOne = streetOne;
            StreetTwo = streetTwo;
            City = city;
            State = state;
            PostalCode = postalCode;
            Country = country;
        }

        public Guid AddressUuid { get; private set; }
        public string StreetOne { get; private set; }
        public string? StreetTwo { get; private set; }
        public string City { get; private set; }
        public string State { get; private set; }
        public string PostalCode { get; private set; }
        public string Country { get; private set; }
    }
}
