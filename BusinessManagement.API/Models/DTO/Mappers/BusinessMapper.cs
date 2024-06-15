using App.Models.DTO.Requests;
using App.Models.ValueObjects;

namespace App.Models.DTO.Mappers
{
    public class BusinessMapper
    {
        public static Business FromRequest(UpdateBusinessInformationRequest req)
        {
            var businessName = new BusinessName(req.BusinessFullname, req.BusinessDisplayName);
            var businessStruc = new BusinessStructure(req.BusinessStructureTypeId, req.CountryCode);

            return new Business(
                req.BusinessUuid,
                req.BusinessOwnerUuid,
                businessName,
                businessStruc,
                req.BusinessIndustry,
                req.IsDeleted
                );

        }
    }
}
