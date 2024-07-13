using App.Models.ValueObjects;

namespace App.Models
{
    public class Customer
    {
        public Customer() { }

        public Customer(Guid customerUuid, CustomerName customerName, CustomerDetail customerDetail, 
            Address customerShippingAddress, Address customerBillingAddress, bool isDeleted)
        {
            if (customerUuid == Guid.Empty)
                throw new ArgumentNullException("Customer uuid was empty", nameof(customerUuid));

            CustomerUuid = customerUuid;
            CustomerDetail = customerDetail;
            CustomerName = customerName;
            CustomerShippingAddress = customerShippingAddress;
            CustomerBillingAddress = customerBillingAddress;
            IsDeleted = isDeleted;
        }


        public void SetCustomerName(CustomerName customerName)
        {
            CustomerName = customerName;
        }

        public void SetCustomerDetail(CustomerDetail customerDetail) 
        {
            CustomerDetail = customerDetail;
        }

        public void SetShippingAddress (Address customerShippingAddress)
        {
            CustomerShippingAddress = customerShippingAddress;
        }

        public void SetBillingAddress(Address customerBillingAddress)
        {
            CustomerBillingAddress = customerBillingAddress;
        }

        public Guid CustomerUuid { get; private set; }
        public CustomerName CustomerName { get; private set; }
        public CustomerDetail? CustomerDetail { get; private set; }
        public Address CustomerShippingAddress { get; private set; }
        public Address CustomerBillingAddress { get; private set; }
        public bool IsDeleted { get; private set; }
    }
}
