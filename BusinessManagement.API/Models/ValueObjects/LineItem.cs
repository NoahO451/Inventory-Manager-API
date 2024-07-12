namespace App.Models.ValueObjects
{
    public record LineItem
    {
        public LineItem() { }

        public LineItem(Guid lineItemUuid, Guid inventoryItemUuid, int orderNumber, string name, int cost, 
            string? description, int quantity)
        {
            if (lineItemUuid == Guid.Empty)
                throw new ArgumentNullException("Line item uuid empty", nameof(lineItemUuid));

            if (inventoryItemUuid == Guid.Empty)
                throw new ArgumentNullException("Inventory item uuid empty", nameof(inventoryItemUuid));

            LineItemUuid = lineItemUuid;
            InventoryItemUuid = inventoryItemUuid;
            OrderNumber = orderNumber;
            Name = name;
            Cost = cost;
            Description = description;
            Quantity = quantity;
        }
        public Guid LineItemUuid { get; private set; }
        public Guid InventoryItemUuid { get; private set; }
        public int OrderNumber { get; private set; }
        public string Name { get; private set; }
        public int Cost { get; private set; } //stored in pennies 
        public string? Description { get; private set; }
        public int Quantity { get; private set; }
    }
}
