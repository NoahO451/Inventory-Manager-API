using App.Models;
using App.Models.DTO;
using App.Models.DTO.Mappers;
using App.Models.DTO.Requests;
using App.Models.DTO.Responses;
using App.Models.ValueObjects;
using App.Repositories;

namespace App.Services
{

    public interface IBusinessService
    {
        Task<ServiceResult<CreateNewBusinessResponse>> CreateNewBusiness(CreateNewBusinessRequest response);
        Task<ServiceResult<GetBusinessResponse>> GetBusiness(Guid uuid);
        Task<ServiceResult> MarkBusinessAsDeleted(Guid uuid);
        Task<ServiceResult<List<GetAllBusinessesResponse>>> GetAllBusnesses(Guid userUuid);
        Task<ServiceResult> UpdateBusinessInformation(UpdateBusinessInformationRequest req);
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

        public async Task<ServiceResult<CreateNewBusinessResponse>> CreateNewBusiness(CreateNewBusinessRequest req)
        {
            try
            {
                Guid NewBusinessUuid = Guid.NewGuid();

                Business newBusiness = new Business(
                    NewBusinessUuid,
                    req.BusinessOwnerUuid,
                    new BusinessName(req.BusinessFullname, req.BusinessDisplayName),
                    new BusinessStructure(req.BusinessStructureTypeId, req.CountryCode),
                    req.BusinessIndustry,
                    false
                );

                bool businessCreated = await _businessrepository.CreateNewBusiness(newBusiness, req.BusinessOwnerUuid);

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

        public async Task<ServiceResult<List<GetAllBusinessesResponse>>> GetAllBusnesses(Guid userUuid)
        {
            List<Business> businesses = await _businessrepository.RetrieveAllBusinesses(userUuid);

            List<GetAllBusinessesResponse> response = new List<GetAllBusinessesResponse>();

            if (businesses == null || businesses.Count == 0)
            {
                _logger.LogWarning("{trace} Businesses null or empty", LogHelper.TraceLog());
                return ServiceResult<List<GetAllBusinessesResponse>>.FailureResult("Businesses null or empty");
            }

            foreach (Business business in businesses)
            {
                response.Add(new GetAllBusinessesResponse
                {
                    BusinessUuid = business.BusinessUuid,
                    BusinessOwnerUuid = business.BusinessOwnerUuid,
                    BusinessFullname = business.BusinessName.BusinessFullName,
                    BusinessDisplayName = business.BusinessName.BusinessDisplayName,
                    BusinessStructureTypeId = business.BusinessStructure.BusinessStructureTypeId,
                    CountryCode = business.BusinessStructure.CountryCode,
                    BusinessIndustry = business.BusinessIndustry,
                });
            }

            return ServiceResult<List<GetAllBusinessesResponse>>.SuccessResult(response);
        }

        public async Task<ServiceResult<GetBusinessResponse>> GetBusiness(Guid uuid)
        {
            Business? business = await _businessrepository.GetBusinessByUuid(uuid);
            
            if (business == null || business.IsDeleted)
            {
                _logger.LogWarning("{trace} Business null or deleted", LogHelper.TraceLog());
                return ServiceResult<GetBusinessResponse>.FailureResult("Error getting the business.");
            }

            var businessResponse = new GetBusinessResponse()
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

            return ServiceResult<GetBusinessResponse>.SuccessResult(businessResponse);
        }

        public async Task<ServiceResult> MarkBusinessAsDeleted(Guid uuid)
        {
            try
            {
                bool businessSetDeleted = await _businessrepository.MarkBusinessAsDeleted(uuid);

                if (businessSetDeleted)
                {
                    return ServiceResult.SuccessResult();
                }

                _logger.LogWarning("{trace} Failed to delete business", LogHelper.TraceLog());
                return ServiceResult.FailureResult("Failed to delete business");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{trace} Exception thrown", LogHelper.TraceLog());
                return ServiceResult.FailureResult("Exception thrown, failed to delete business", ex);
            }
        }

        public async Task<ServiceResult> UpdateBusinessInformation(UpdateBusinessInformationRequest req)
        {
            try
            {
                Business business = BusinessMapper.FromRequest(req);

                bool businessUpdated = await _businessrepository.UpdateBusinessInformation(business);

                if (businessUpdated)
                {
                    return ServiceResult.SuccessResult();
                }

                _logger.LogWarning("{trace} Failed to update business", LogHelper.TraceLog());
                return ServiceResult.FailureResult("Failed to update business");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{trace} Exception thrown", LogHelper.TraceLog());
                return ServiceResult.FailureResult("Exception thrown, failed to update business", ex);
            }
        }
    }
}
