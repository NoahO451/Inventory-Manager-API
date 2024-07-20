using App.Models.ValueObjects;

namespace App.Models
{
    public class SalesOrder
    {
        public SalesOrder() { }

        public SalesOrder(Guid salesOrderUuid, string referenceNumber, Guid? sOCustomerUuid, Guid businessUuid, 
            DateTime createdAt, bool isTaxInclusive, SalesOrderDetail salesOrderDetail, Address sOShippingAddress, 
            Address sOBillingAddress, List<LineItem> lineItems)
        {
            if (salesOrderUuid == Guid.Empty)
                throw new ArgumentNullException("Sales order uuid empty", nameof(salesOrderUuid));

            if (lineItems == null)
                throw new ArgumentNullException("Line items are null", nameof(lineItems));

            if (createdAt < DateTime.Now)
                throw new ArgumentException("Created at time cannot be in the past", nameof(createdAt));


            SalesOrderUuid = salesOrderUuid;
            ReferenceNumber = referenceNumber;
            SOCustomerUuid = sOCustomerUuid;
            CreatedAt = createdAt;
            IsTaxInclusive = isTaxInclusive;
            SalesOrderDetail = salesOrderDetail;
            SOShippingAddress = sOShippingAddress;
            SOBillingAddress = sOBillingAddress;
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

        public void SetShippingAddress(Address sOShippingAddress)
        {
            SOShippingAddress = sOShippingAddress;
        }

        public void SetBillingAddress(Address sOBillingAddress)
        {
            SOBillingAddress = sOBillingAddress;
        }

        public Guid SalesOrderUuid { get; private set; }
        public string ReferenceNumber { get; private set; }
        public Guid? SOCustomerUuid { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public bool IsTaxInclusive { get; private set; }
        public SalesOrderDetail SalesOrderDetail { get; private set; }
        public Address SOShippingAddress { get; private set; }
        public Address SOBillingAddress { get; private set; }
        public List<LineItem> LineItems { get; private set; }
    }
}
