using App.Models;
using App.Models.ValueObjects;
using Dapper;

namespace App.Repositories
{
    public interface IBusinessRepository
    {
        Task<bool> CreateNewBusiness(Business business);
        Task<bool> MarkBusinessAsDeleted(Guid uuid);
        Task<Business?> GetBusinessByUuid(Guid uuid);
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
                 INSERT INTO business (business_uuid, business_owner_uuid, business_fullname, business_display_name, business_structure_type_id, country_code, business_industry, is_deleted)
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

        /// <summary>
        /// Returns business with matching uuid
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns></returns>
        public async Task<Business?> GetBusinessByUuid(Guid uuid)
        {
            using (var connection = _context.CreateConnection())
            {
                string sql = """
                    SELECT 
                        business_uuid AS BusinessUuid, 
                        business_owner_uuid AS BusinessOwnerUuid,
                        business_industry AS BusinessIndustry, 
                        is_deleted AS IsDeleted,

                        business_fullname AS BusinessFullname, 
                        business_display_name AS BusinessDisplayName,

                        business_structure_type_id AS BusinessStructureTypeId, 
                        country_code AS CountryCode

                    FROM 
                        business
                    WHERE business_uuid = @BusinessUuid;
                    """;

                var business = await connection.QueryAsync<Business, BusinessName, BusinessStructure, Business>(
                    sql,
                    (business, name, structure) =>
                    {
                        if (business.BusinessUuid == Guid.Empty)
                            throw new ArgumentException("Uuid empty", nameof(business));
                        business.BusinessName = name;
                        business.BusinessStructure = structure;
                        return business;
                    },
                    new { BusinessUuid =  uuid },
                    splitOn: "BusinessFullname,BusinessStructureTypeId"
                    );

                return business?.FirstOrDefault();
            }
        }

        public async Task<bool> MarkBusinessAsDeleted(Guid uuid)
        {
            throw new NotImplementedException();
        }
    }
}
