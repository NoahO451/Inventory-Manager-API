namespace App.Models.ValueObjects
{
    public record SalesOrderDetail
    {
        public SalesOrderDetail() { }

        public SalesOrderDetail(string? deliveryMethod, string? notes, string? terms, int?
            shippingCharge, int? adjustment, string? adjustmentDescription)
        {
            DeliveryMethod = deliveryMethod;
            Notes = notes;
            Terms = terms;
            ShippingCharge = shippingCharge;
            Adjustment = adjustment;
            AdjustmentDescription = adjustmentDescription;
        }

        public string? DeliveryMethod { get; private set; }
        public string? Notes { get; private set; }
        public string? Terms { get; private set; }
        public int? ShippingCharge { get; private set; } //stored in pennies
        public int? Adjustment { get; private set; }
        public string? AdjustmentDescription { get; private set; }
    }
}
