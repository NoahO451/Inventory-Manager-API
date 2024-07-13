using System.ComponentModel.DataAnnotations;

namespace App.Models.ValueObjects
{
    public record CustomerDetail
    {
        public CustomerDetail () { }

        public CustomerDetail(string? company, string? phoneNumber, string? email)
        {
            Company = company;
            PhoneNumber = phoneNumber;
            Email = email;
        } 

        public string? Company { get; private set; }
        public string? PhoneNumber { get; private set; }
        public string? Email { get; private set; }
    }
}
