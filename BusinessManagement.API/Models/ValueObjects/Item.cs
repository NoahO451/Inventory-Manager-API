namespace App.Models.ValueObjects
{
    public record Item
    {
        // Empty constructor is needed for Dapper to work
        public Item() { }
        
        public Item(string name, string? description, decimal? cost, int quantity, DateTime? expirationDate, int? category, int? itemWeightG)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Item must have a name", nameof(name));

            if (quantity < 0)
                throw new ArgumentException("Item must have a quantity", nameof(quantity));

            Name = name;
            Description = description;
            Cost = cost;
            Quantity = quantity;
            ExpirationDate = expirationDate;
            Category = category;
            ItemWeightG = itemWeightG;
        }

        public string Name { get; private set; }
        public string? Description { get; private set; }
        public decimal? Cost { get; private set; }
        public int Quantity { get; private set; }
        public DateTime? ExpirationDate { get; private set; }
        public int? Category { get; private set; }
        public int? ItemWeightG { get; private set; } 
    }
}