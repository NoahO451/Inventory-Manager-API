using App.Models;
using App.Models.ValueObjects;
using Dapper;
using System;

namespace App.Repositories
{
    public interface IBusinessRepository
    {
        Task<bool> CreateNewBusiness(Business business, Guid ownerUuid);
        Task<bool> MarkBusinessAsDeleted(Guid uuid);
        Task<Business?> GetBusinessByUuid(Guid uuid);
        Task<List<Business>> RetrieveAllBusinesses(Guid userId);
        Task<bool> UpdateBusinessInformation (Business business);
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
        public async Task<bool> CreateNewBusiness(Business business, Guid ownerUuid)
        {
            try
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
                        RETURNING business_id;
                        """;

                    int businessId = await connection.QueryFirstOrDefaultAsync<int>(sql, parameters);

                    string getOwnerIdSql = """
                    SELECT 
                        user_id
                    FROM 
                        user_data u
                    WHERE
                        u.user_uuid = @OwnerUuid;
                    """;

                    int ownerId = await connection.QueryFirstOrDefaultAsync<int>(getOwnerIdSql, new { OwnerUuid = ownerUuid });

                    string insertUserBusinessSql = """
                    INSERT INTO user_business (user_id, business_id)
                    VALUES (@OwnerId, @BusinessId);
                    """;

                    await connection.ExecuteAsync(insertUserBusinessSql, new { OwnerId = ownerId, BusinessId = businessId });

                    return true;
                }

            }
            catch (Exception)
            {
                _logger.LogWarning("{trace} no database rows affected", LogHelper.TraceLog());
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
                    new { BusinessUuid = uuid },
                    splitOn: "BusinessFullname,BusinessStructureTypeId"
                    );

                return business?.FirstOrDefault();
            }
        }

        public async Task<bool> MarkBusinessAsDeleted(Guid uuid)
        {
            using (var connection = _context.CreateConnection())
            {
                string sql = """
                    UPDATE 
                        business
                    SET 
                        is_deleted = true
                    WHERE 
                        business_uuid = @BusinessUuid;
                    """;

                int rowsUpdated = await connection.ExecuteAsync(sql, new { BusinessUuid = uuid });

                if (rowsUpdated > 0)
                {
                    return true;
                }

                _logger.LogWarning("{trace} no datebase rows affected", LogHelper.TraceLog());
                return false;
            }
        }

        public async Task<List<Business>> RetrieveAllBusinesses(Guid userUuid)
        {
            IEnumerable<Business>? result = null;

            using (var connection = _context.CreateConnection())
            {
                string sql = """
                     SELECT 
                        business_uuid AS BusinessUuid, 
                        business_owner_uuid AS BusinessOwnerUuid,
                        business_industry AS BusinessIndustry, 
                        b.is_deleted AS IsDeleted,

                        business_fullname AS BusinessFullname, 
                        business_display_name AS BusinessDisplayName,

                        business_structure_type_id AS BusinessStructureTypeId, 
                        country_code AS CountryCode,

                        u.user_id AS UserID,
                        u.user_uuid AS UserUuid,
                        u.full_name AS UserName
                     FROM 
                        business b
                     LEFT JOIN 
                        user_business ub ON b.business_id = ub.business_id
                     LEFT JOIN 
                        user_data u ON ub.user_id = u.user_id
                     WHERE b.is_deleted = false AND u.user_uuid = @UserUuid;
                    """;
                result = await connection.QueryAsync<Business, BusinessName, BusinessStructure, Business>(
                    sql,
                    (business, name, structure) =>
                    {
                        if (business.BusinessUuid == Guid.Empty)
                            throw new ArgumentException("uuid empty", nameof(business));
                        business.BusinessName = name;
                        business.BusinessStructure = structure;
                        return business;
                    },
                    new { UserUuid = userUuid },
                    splitOn: "BusinessFullname,BusinessStructureTypeId"
                    );

                return result.ToList();
            }
        }
        /// <summary>
        /// Update business information in the database
        /// </summary>
        /// <param name="business"></param>
        /// <returns></returns>
        public async Task<bool> UpdateBusinessInformation(Business business)
        {
            using (var connection = _context.CreateConnection())
            {
                string sql = """
                    UPDATE 
                        business
                    SET
                        business_fullname = @Fullname,
                        business_display_name = @DisplayName,
                        business_structure_type_id = @StructureTypeId,
                        country_code = @CountryCode,
                        business_industry = @Industry
                    WHERE
                        business_uuid = @Uuid AND business_owner_uuid = @OwnerUuid;
                    """;
                var parameters = new
                {
                    Uuid = business.BusinessUuid,
                    OwnerUuid = business.BusinessOwnerUuid,
                    Fullname = business.BusinessName.BusinessFullName,
                    DisplayName = business.BusinessName.BusinessDisplayName,
                    StructureTypeId = business.BusinessStructure.BusinessStructureTypeId,
                    CountryCode = business.BusinessStructure.CountryCode,
                    Industry = business.BusinessIndustry
                };

                int rowsUpdated = await connection.ExecuteAsync(sql, parameters);

                if (rowsUpdated > 0)
                {
                    return true;
                }

                _logger.LogWarning("{trace} no database rows affected", LogHelper.TraceLog());
                return false;
            }
        }
    }
}
