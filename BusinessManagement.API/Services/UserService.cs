using App.Models;
using App.Models.DTO.Requests;
using App.Models.DTO.Responses;
using App.Models.ValueObjects;
using App.Repositories;

namespace App.Services
{
    public interface IUserService
    {
        Task<ApiResponse<NewUserSignupResponse>> NewUserSignup(NewUserSignupRequest request);
        Task<ApiResponse<GetUserResponse>> GetUser(Guid uuid);

    }

    public class UserService : IUserService
    {
        private IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<ApiResponse<NewUserSignupResponse>> NewUserSignup(NewUserSignupRequest req)
        {
            try
            {
                Guid NewUserUuid = Guid.NewGuid();

                // Map the request to a new UserData model. The model validates itself, so any issues with the 
                // request (invalid username, invalid name, etc.) should be caught here. 
                UserData newUser = new UserData(
                    NewUserUuid,
                    new Auth0Id(req.FullAuth0Id),
                    new Name(req.FirstName, req.LastName),
                    new Email(req.Email),
                    null,
                    DateTime.UtcNow,
                    DateTime.UtcNow,
                    false,
                    false
                );

                bool userCreated = await _userRepository.CreateNewUser(newUser);

                var apiResponse = new ApiResponse<NewUserSignupResponse>();

                if (!userCreated)
                {
                    apiResponse.Message = "There was an error saving to the database.";
                    apiResponse.Success = false;
                    return apiResponse;
                }

                var newUserResponse = new NewUserSignupResponse()
                {
                    UserUuid = newUser.UserUuid,
                    FirstName = req.FirstName,
                    LastName = req.LastName,
                    Email = newUser.Email.EmailAddress,
                    IsPremiumMember = newUser.IsPremiumMember
                };

                apiResponse.Data = newUserResponse;
                apiResponse.Success = true;
                return apiResponse;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<ApiResponse<GetUserResponse>> GetUser(Guid uuid)
        {
            UserData user = await _userRepository.GetUserByUuid(uuid);
            ApiResponse<GetUserResponse> apiResponse = new ApiResponse< GetUserResponse>();

            if (user == null)
            {
                apiResponse.Success = false;
                apiResponse.Message = "Error getting the user.";
                return apiResponse;
            }

            var userResponse = new GetUserResponse()
            {
                UserUuid = user.UserUuid,
                Auth0UserId = user.Auth0Id.Auth0UserId,
                FirstName = user.Name.FirstName, 
                LastName = user.Name.LastName,
                Businesses = user.Businesses,
                LastLogin = user.LastLogin,
                IsPremiumMember = user.IsPremiumMember, 
                IsDeleted = user.IsDeleted
            };

            apiResponse.Success = true;
            apiResponse.Data = userResponse;

            return apiResponse;

        }
    }
}
