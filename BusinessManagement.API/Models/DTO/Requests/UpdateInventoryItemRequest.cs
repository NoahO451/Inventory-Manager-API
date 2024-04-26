namespace App.Models.DTO.Requests
{
    public class UpdateInventoryItemRequest
    {
        public Guid InventoryItemUuid { get; set; }
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
        public Guid? CustomPackageUuid { get; set; }
        public int? ItemWeightG { get; set; } // weight is stored in grams in the database
        public bool IsListed { get; set; }
        public bool IsLot { get; set; }
        public string? Notes { get; set; }
        public Guid BusinessUuid { get; set; }
        public Guid UserUuid { get; set; }
    }
}