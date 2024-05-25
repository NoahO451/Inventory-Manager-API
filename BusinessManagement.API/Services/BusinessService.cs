using App.Models;
using App.Models.DTO;
using App.Models.DTO.Responses;
using App.Models.ValueObjects;
using App.Repositories;

namespace App.Services
{

    public interface IBusinessService
    {
        Task<ServiceResult<BusinessResponse>> CreateNewBusiness(BusinessResponse response);
        Task<ServiceResult<BusinessResponse>> GetBusiness(Guid uuid);
    }
    public class BusinessService : IBusinessService
    {
        private IBusinessRepository _businessrepository;

        private ILogger<BusinessService> _logger;

        public BusinessService(IBusinessRepository businessrepository, ILogger<BusinessService> logger)
        {
            _businessrepository = businessrepository;
            _logger = logger;
        }

        public async Task<ServiceResult<BusinessResponse>> CreateNewBusiness(BusinessResponse response)
        {
            try
            {
                Guid NewBusinessUuid = Guid.NewGuid();

                Business newBusiness = new Business(
                    NewBusinessUuid,
                    response.BusinessOwnerUuid,
                    new BusinessName(response.BusinessFullname, response.BusinessDisplayName),
                    new BusinessStructure(response.BusinessStructureTypeId, response.CountryCode),
                    response.BusinessIndustry,
                    false
                );

                bool businessCreated = await _businessrepository.CreateNewBusiness(newBusiness);

                if (!businessCreated)
                {
                    _logger.LogWarning("{trace} Failed to create new business", LogHelper.TraceLog());
                    return ServiceResult<BusinessResponse>.FailureResult("There was an error saving to the database.");
                }

                var newBusinessResponse = new BusinessResponse()
                {
                    BusinessUuid = newBusiness.BusinessUuid,
                    BusinessOwnerUuid = newBusiness.BusinessOwnerUuid,
                    BusinessFullname = newBusiness.BusinessName.BusinessFullName,
                    BusinessDisplayName = newBusiness.BusinessName.BusinessDisplayName,
                    BusinessStructureTypeId = newBusiness.BusinessStructure.BusinessStructureTypeId,
                    CountryCode = newBusiness.BusinessStructure.CountryCode,
                    BusinessIndustry = newBusiness.BusinessIndustry,
                };

                return ServiceResult<BusinessResponse>.SuccessResult(newBusinessResponse);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{trace} Exception Thrown", LogHelper.TraceLog());
                throw;
            }
        }

        public async Task<ServiceResult<BusinessResponse>> GetBusiness(Guid uuid)
        {
            Business? business = await _businessrepository.GetBusinessByUuid(uuid);
            
            if (business == null || business.IsDeleted)
            {
                _logger.LogWarning("{trace} Business null or deleted", LogHelper.TraceLog());
                return ServiceResult<BusinessResponse>.FailureResult("Error getting the business.");
            }

            var businessResponse = new BusinessResponse()
            {
                BusinessUuid = business.BusinessUuid,
                BusinessOwnerUuid = business.BusinessOwnerUuid,
                BusinessFullname = business.BusinessName.BusinessFullName,
                BusinessDisplayName = business.BusinessName.BusinessDisplayName,
                BusinessStructureTypeId = business.BusinessStructure.BusinessStructureTypeId,
                CountryCode = business.BusinessStructure.CountryCode,
                BusinessIndustry = business.BusinessIndustry,
                IsDeleted = business.IsDeleted
            };

            return ServiceResult<BusinessResponse>.SuccessResult(businessResponse);
        }
    }
}
