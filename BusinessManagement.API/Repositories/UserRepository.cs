using App.Helpers;
using App.Models;
using App.Models.ValueObjects;
using Dapper;

namespace App.Repositories
{
    public interface IUserRepository
    {
        Task<bool> CreateNewUser(UserData user);
        Task<UserData> GetUserByUuid(Guid uuid);
        

    }

    public class UserRepository : IUserRepository
    {
        private DataContext _context;

        public UserRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<bool> CreateNewUser(UserData user)
        {
            int affectedRows = 0;

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
                    AuthProvider = user.Auth0Id.AuthProvider
                };

                string sql = """
                    INSERT INTO user_data (user_uuid, auth0_id, first_name, last_name, email, created_at, last_login, is_premium_member, is_deleted, auth_provider) 
                    VALUES (@UserUuid, @Auth0Id, @FirstName, @LastName, @Email, @CreatedAt, @LastLogin, @IsPremiumMember, @IsDeleted, @AuthProvider);
                    """;

                affectedRows = await connection.ExecuteAsync(sql, parameters);

            }

            if (affectedRows == 0)
            {
                return false;
            }

            return true;
        }

        public async Task<UserData> GetUserByUuid(Guid uuid)
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
                        auth_provider AS AuthProvider,

                        first_name AS FirstName,
                        last_name AS LastName,

                        email AS EmailAddress
                    FROM 
                        user_data
                    WHERE user_uuid = @UserUuid;                    
                    """;

                var test = await connection.QueryAsync<UserData, Auth0Id, Name, Email, UserData>(
                    sql, 
                    (userData, auth0Id, name, email) =>
                    {
                        string fullAuth0Id = $"{auth0Id.AuthProvider}|{auth0Id.Auth0UserId}";

                        userData.SetAuth0Id(fullAuth0Id);
                        userData.SetName(name.FirstName, name.LastName);
                        userData.SetEmail(email.EmailAddress);

                        return userData;
                    }, 
                    new { UserUuid = uuid }, 
                    splitOn: "Auth0UserId,FirstName,EmailAddress"
                    );

                return test.FirstOrDefault();
            }
        }

    }
}
