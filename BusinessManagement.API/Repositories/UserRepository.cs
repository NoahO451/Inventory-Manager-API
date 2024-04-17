﻿using App.Helpers;
using App.Models;
using App.Models.ValueObjects;
using Dapper;

namespace App.Repositories
{
    public interface IUserRepository
    {
        Task<bool> CreateNewUser(UserData user);
        Task<UserData?> GetUserByUuid(Guid uuid);
        Task<bool> UpdateUserDemographics(UserData user);
        Task<bool> MarkUserAsDeleted(Guid uuid);
    }

    /// <summary>
    /// Handles database queries for user entities
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private DataContext _context;

        public UserRepository(DataContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Creates a new user. Intended for use immediately after a user's Auth0 signup is complete.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<bool> CreateNewUser(UserData user)
        {
            using (var connection = _context.CreateConnection())
            {
                var parameters = new
                {
                    UserUuid = user.UserUuid,
                    Auth0Id = user.Auth0Id.Auth0UserId,
                    FirstName = user.Name.FirstName,
                    LastName = user.Name.LastName,
                    Email = user.Email.EmailAddress,
                    CreatedAt = user.CreatedAt,
                    LastLogin = user.LastLogin,
                    IsPremiumMember = user.IsPremiumMember,
                    IsDeleted = user.IsDeleted,
                };

                string sql = """
                    INSERT INTO user_data (user_uuid, auth0_id, first_name, last_name, email, created_at, last_login, is_premium_member, is_deleted) 
                    VALUES (@UserUuid, @Auth0Id, @FirstName, @LastName, @Email, @CreatedAt, @LastLogin, @IsPremiumMember, @IsDeleted);
                    """;

                int affectedRows = await connection.ExecuteAsync(sql, parameters);

                if (affectedRows > 0)
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Get user by uuid from the database
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns></returns>
        public async Task<UserData?> GetUserByUuid(Guid uuid)
        {
            using (var connection = _context.CreateConnection())
            {
                string sql = """
                    SELECT 
                        user_uuid AS UserUuid,
                        created_at AS CreatedAt,
                        last_login AS LastLogin,
                        is_premium_member AS IsPremiumMember,
                        is_deleted AS IsDeleted,

                        auth0_id AS Auth0UserId,

                        first_name AS FirstName,
                        last_name AS LastName,

                        email AS EmailAddress
                    FROM 
                        user_data
                    WHERE user_uuid = @UserUuid;                    
                    """;

                var user = await connection.QueryAsync<UserData, Auth0Id, Name, Email, UserData>(
                    sql,
                    (userData, auth0Id, name, email) =>
                    {
                        userData.SetAuth0Id(auth0Id.Auth0UserId);
                        userData.SetName(name.FirstName, name.LastName);
                        userData.SetEmail(email.EmailAddress);

                        return userData;
                    },
                    new { UserUuid = uuid },
                    splitOn: "Auth0UserId,FirstName,EmailAddress"
                    );

                return user?.FirstOrDefault();
            }
        }

        /// <summary>
        /// Update user demographic information in database
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<bool> UpdateUserDemographics(UserData user)
        {
            using (var connection = _context.CreateConnection())
            {
                string sql = """
                    UPDATE 
                        user_data
                    SET 
                        first_name = @FirstName, 
                        last_name = @LastName, 
                        email = @EmailAddress
                    WHERE 
                        user_uuid = @Uuid
                    """;

                var parameters = new
                {
                    Uuid = user.UserUuid,
                    FirstName = user.Name.FirstName,
                    LastName = user.Name.LastName,
                    EmailAddress = user.Email.EmailAddress
                };

                int rowsUpdated = await connection.ExecuteAsync(sql, parameters);

                if (rowsUpdated > 0)
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Set a user's IsDeleted flag to true
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns></returns>
        public async Task<bool> MarkUserAsDeleted(Guid uuid)
        {
            using (var connection = _context.CreateConnection())
            {
                string sql = """
                    UPDATE 
                        user_data
                    SET 
                        is_deleted = true
                    WHERE 
                        user_uuid = @UserUuid;
                    """;

                int rowsUpdated = await connection.ExecuteAsync(sql, new { UserUuid = uuid });

                if (rowsUpdated > 0)
                {
                    return true;
                }

                return false;
            }
        }
    }
}