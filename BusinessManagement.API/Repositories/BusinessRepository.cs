using App.Models;
using Dapper;

namespace App.Repositories
{
    public interface IBusinessRepository
    {
        Task<bool> CreateNewBusiness(Business business);
    }

    /// <summary>
    /// Handles database queries for user entities
    /// </summary>
    public class BusinessRepository : IBusinessRepository
    {
        private DataContext _context;
        private ILogger<BusinessRepository> _logger;

        public BusinessRepository(DataContext context, ILogger<BusinessRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new business using users uuid to set the owner. 
        /// </summary>
        /// <param name="business"></param>
        /// <param name="ownerUuid"></param>
        /// <returns></returns>
        public async Task<bool> CreateNewBusiness(Business business)
        {
            using (var connection = _context.CreateConnection())
            {
                var parameters = new
                {
                    BusinessUuid = business.BusinessUuid,
                    OwnerUuid = business.BusinessOwnerUuid,
                    BusinessFullname = business.BusinessName.BusinessFullName,
                    BusinessDisplayName = business.BusinessName.BusinessDisplayName,
                    BusinessStructureTypeId = business.BusinessStructure.BusinessStructureTypeId,
                    CountryCode = business.BusinessStructure.CountryCode,
                    BusinessIndustry = business.BusinessIndustry,
                    IsDeleted = business.IsDeleted,
                };

                string sql = """
                 INSERT INTO business b (business_uuid, business_owner_uuid, business_fullname, business_display_name, business_structure_type_id, country_code, business_industry, is_deleted)
                 VALUES (@BusinessUuid, @OwnerUuid, @BusinessFullname, @BusinessDisplayName, @BusinessStructureTypeId, @CountryCode, @BusinessIndustry, @IsDeleted)
                """;

                int affectedRows = await connection.ExecuteAsync(sql, parameters);
                
                if (affectedRows > 0)
                {
                    return true;
                }

                _logger.LogWarning("{trace} No database rows affected", LogHelper.TraceLog());
                return false;
            }
        }
    }
}
