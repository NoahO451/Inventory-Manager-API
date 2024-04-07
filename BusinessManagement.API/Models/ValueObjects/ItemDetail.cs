
namespace App.Models.ValueObjects
{
    public record ItemDetail
    {
        // Empty constructor is needed for Dapper to work
        public ItemDetail() { }

        public ItemDetail(string? sku, string? serialNumber, string? supplier, string? brand, string? model)
        {
            SKU = sku;
            SerialNumber = serialNumber;
            Supplier = supplier;
            Brand = brand;
            Model = model;
        }

        public string? SKU {  get; private set; } 
        public string? SerialNumber { get; private set; }
        public string? Supplier { get; private set; }
        public string? Brand { get; private set; }
        public string? Model { get; private set; }
    }
}