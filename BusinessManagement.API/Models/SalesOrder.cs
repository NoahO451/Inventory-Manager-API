using App.Models.ValueObjects;

namespace App.Models
{
    public class SalesOrder
    {
        public SalesOrder() { }

        public SalesOrder(Guid salesOrderUuid, string referenceNumber, Guid sOCustomerUuid, Guid businessUuid, 
            DateTime createdAt, bool isTaxInclusive, SalesOrderDetail salesOrderDetail, Guid shippingAddressUuid, Guid billingAddressUuid, 
            List<LineItem> lineItems)
        {
            if (salesOrderUuid == Guid.Empty)
                throw new ArgumentNullException("Sales order uuid empty", nameof(salesOrderUuid));

            if (sOCustomerUuid == Guid.Empty)
                throw new ArgumentNullException("Customer uuid empty", nameof(sOCustomerUuid));

            if (businessUuid == Guid.Empty)
                throw new ArgumentNullException("Business uuid empty", nameof(businessUuid));

            if (shippingAddressUuid == Guid.Empty)
                throw new ArgumentNullException("Shipping address uuid empty", nameof(shippingAddressUuid));

            if (billingAddressUuid == Guid.Empty)
                throw new ArgumentNullException("Billing address uuid empty", nameof(billingAddressUuid));

            if (lineItems == null)
                throw new ArgumentNullException("Line items are null", nameof(lineItems));

            if (createdAt < DateTime.Now)
                throw new ArgumentException("Created at time cannot be in the past", nameof(createdAt));


            SalesOrderUuid = salesOrderUuid;
            ReferenceNumber = referenceNumber;
            SOCustomerUuid = sOCustomerUuid;
            BusinessUuid = businessUuid;
            CreatedAt = createdAt;
            IsTaxInclusive = isTaxInclusive;
            SalesOrderDetail = salesOrderDetail;
            ShippingAddressUuid = shippingAddressUuid;
            BillingAddressUuid = billingAddressUuid;
            LineItems = lineItems;
        }

        public void SetSalesOrderDetail(SalesOrderDetail salesOrderDetail)
        {
            SalesOrderDetail = salesOrderDetail;
        }

        public void SetLineItem(List<LineItem> lineItems)
        {
            LineItems = lineItems;
        }

        public Guid SalesOrderUuid { get; private set; }
        public string ReferenceNumber { get; private set; }
        public Guid SOCustomerUuid { get; private set; }
        public Guid BusinessUuid { get; private set; } //might not need this
        public DateTime CreatedAt { get; private set; }
        public bool IsTaxInclusive { get; private set; }
        public SalesOrderDetail SalesOrderDetail { get; private set; }
        public Guid ShippingAddressUuid { get; private set; }
        public Guid BillingAddressUuid { get; private set; }
        public List<LineItem> LineItems { get; private set; }
    }
}
