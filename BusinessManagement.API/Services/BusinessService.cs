using App.Models;
using App.Models.DTO;
using App.Models.DTO.Responses;
using App.Models.ValueObjects;
using App.Repositories;

namespace App.Services
{

    public interface IBusinessService
    {
        Task<ServiceResult<CreateNewBusinessResponse>> CreateNewBusiness(CreateNewBusinessResponse response);
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

        public async Task<ServiceResult<CreateNewBusinessResponse>> CreateNewBusiness(CreateNewBusinessResponse response)
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
                    return ServiceResult<CreateNewBusinessResponse>.FailureResult("There was an error saving to the database.");
                }

                var newBusinessResponse = new CreateNewBusinessResponse()
                {
                    BusinessUuid = newBusiness.BusinessUuid,
                    BusinessOwnerUuid = newBusiness.BusinessOwnerUuid,
                    BusinessFullname = newBusiness.BusinessName.BusinessFullName,
                    BusinessDisplayName = newBusiness.BusinessName.BusinessDisplayName,
                    BusinessStructureTypeId = newBusiness.BusinessStructure.BusinessStructureTypeId,
                    CountryCode = newBusiness.BusinessStructure.CountryCode,
                    BusinessIndustry = newBusiness.BusinessIndustry,
                };

                return ServiceResult<CreateNewBusinessResponse>.SuccessResult(newBusinessResponse);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{trace} Exception Thrown", LogHelper.TraceLog());
                throw;
            }
        }
    }
}
