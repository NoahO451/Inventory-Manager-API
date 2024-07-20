namespace App.Models.ValueObjects
{
    public record Address
    {
        public Address () { }

        public Address(string streetOne, string? streetTwo, string city, string state, 
            string postalCode, string country)
        {
            StreetOne = streetOne;
            StreetTwo = streetTwo;
            City = city;
            State = state;
            PostalCode = postalCode;
            Country = country;
        }

        public string StreetOne { get; private set; }
        public string? StreetTwo { get; private set; }
        public string City { get; private set; }
        public string State { get; private set; }
        public string PostalCode { get; private set; }
        public string Country { get; private set; }
    }
}
