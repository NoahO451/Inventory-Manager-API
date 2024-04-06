namespace App.Models.DTO.Requests
{
    public class AddInventoryItemRequest
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? SKU { get; set; }
        public decimal? Cost { get; set; }
        public string? SerialNumber { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string? Supplier { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public int Quantity { get; set; }
        public int? ReorderQuantity { get; set; }
        public string? Location { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public int? Category { get; set; }
        public int? CustomPackageId { get; set; }
        public int? ItemWeightG { get; set; } // weight is stored in grams in the database
        public bool IsListed { get; set; }
        public bool IsLot { get; set; }
        public string? Notes { get; set; }
        public int BusinessId { get; set; }
        public int UserId { get; set; }
    }
}
