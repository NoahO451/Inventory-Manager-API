using App.Models.ValueObjects;

namespace App.Models
{
    public class Business
    {
        public Business() { }
        public Business(Guid businessUuid, Guid businessOwnerUuid, BusinessName businessName, BusinessStructure businessStructure, string businessIndustry, bool isDeleted)
        {
            if (businessUuid == Guid.Empty)
                throw new ArgumentException("Business uuid empty", nameof(businessUuid));

            if (businessOwnerUuid == Guid.Empty)
                throw new ArgumentException("Owner uuid empty", nameof(businessOwnerUuid));

            if (isDeleted)
                throw new ArgumentException("Business is deleted", nameof(isDeleted));

            BusinessUuid = businessUuid;
            BusinessOwnerUuid = businessOwnerUuid;
            BusinessName = businessName;
            BusinessStructure = businessStructure;
            BusinessIndustry = businessIndustry;
            IsDeleted = isDeleted;
        }

        public void SetBusinessName(BusinessName businessName)
        {
            BusinessName = businessName;
        }

        public void SetBusinessStructure(BusinessStructure businessStructure)
        {
            BusinessStructure = businessStructure;
        }

        public Guid BusinessUuid { get; private set; }
        public Guid BusinessOwnerUuid { get; private set; }
        public BusinessName BusinessName { get; private set; }
        public BusinessStructure BusinessStructure { get; private set; }
        public string BusinessIndustry { get; private set; }
        public bool IsDeleted { get; private set; }
    }
}