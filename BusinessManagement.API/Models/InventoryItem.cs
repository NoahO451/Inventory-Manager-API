using App.Models.ValueObjects;

namespace App.Models
{
    public class InventoryItem
    {
        public InventoryItem(Guid inventoryItemUuid, DateTime purchaseDate, int? reorderQuantity, int? customPackageId,
                                  string? location, bool isListed, bool isLot, string? notes, Item item, ItemDetail itemDetail)
        {
            InventoryItemUuid = inventoryItemUuid;
            PurchaseDate = purchaseDate;
            ReorderQuantity = reorderQuantity;
            CustomPackageId = customPackageId;
            Location = location;
            IsListed = isListed;
            IsLot = isLot;
            Notes = notes;
            Item = item;
            ItemDetail = itemDetail;
        }

        public Guid InventoryItemUuid { get; private set; }
        public DateTime? PurchaseDate { get; private set; }
        public int? ReorderQuantity { get; private set; }
        public int? CustomPackageId { get; private set; } 
        public string? Location { get; private set; } 
        public bool IsListed { get; private set; }
        public bool IsLot { get; private set; }
        public string? Notes { get; private set; }
        public Item Item { get; private set; }
        public ItemDetail ItemDetail { get; private set; }
    }
}
