using App.Models;
using App.Models.DTO;
using App.Models.DTO.Requests;
using App.Models.DTO.Responses;
using App.Models.ValueObjects;
using App.Repositories;

namespace App.Services
{
    public interface IUserService
    {
        Task<ApiResponse<NewUserSignupResponse>> NewUserSignup(NewUserSignupRequest request);
        Task<ServiceResult<GetUserResponse>> GetUser(Guid uuid);
        Task<ServiceResult<UpdateUserDemographicsResponse>> UpdateUserDemographics(UpdateUserDemographicsRequest request);
        Task<ServiceResult> MarkUserAsDeleted(Guid uuid);
    }

    public class UserService : IUserService
    {
        private IUserRepository _userRepository;

        private IAuth0Service _auth0Service;
        public UserService(IUserRepository userRepository, IAuth0Service auth0Service)
        {
            _userRepository = userRepository;
            _auth0Service = auth0Service;
        }

        public async Task<ApiResponse<NewUserSignupResponse>> NewUserSignup(NewUserSignupRequest req)
        {
            try
            {
                Guid NewUserUuid = Guid.NewGuid();

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

        public async Task<ServiceResult<GetUserResponse>> GetUser(Guid uuid)
        {
            UserData? user = await _userRepository.GetUserByUuid(uuid);

            if (user == null || user.IsDeleted)
            {
                return ServiceResult<GetUserResponse>.FailureResult("Error getting the user.");
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

            return ServiceResult<GetUserResponse>.SuccessResult(userResponse);
        }

        public async Task<ServiceResult<UpdateUserDemographicsResponse>> UpdateUserDemographics(UpdateUserDemographicsRequest req)
        {
            try
            {
                UserData? user = await _userRepository.GetUserByUuid(req.UserUuid);

                if (user == null)
                {
                    return ServiceResult<UpdateUserDemographicsResponse>.FailureResult("User with this uuid not found.");
                }

                bool updateRequired = false;

                string? previousEmail = null;

                if (user.Name.FirstName != req.FirstName || user.Name.LastName != req.LastName)
                {
                    user.SetName(req.FirstName, req.LastName);
                    updateRequired = true;
                }

                if (user.Email.EmailAddress != req.EmailAddress)
                {
                    ServiceResult auth0UpdateResult = await _auth0Service.UpdateAuth0UserEmail(user.Auth0Id.Auth0UserId, req.EmailAddress);

                    if (!auth0UpdateResult.Success)
                    {
                        return ServiceResult<UpdateUserDemographicsResponse>.FailureResult(auth0UpdateResult.ErrorMessage ?? "Failed to update Auth0 email.");
                    }

                    user.SetEmail(req.EmailAddress);
                    updateRequired = true;
                }

                if (updateRequired)
                {
                    bool userUpdated = await _userRepository.UpdateUserDemographics(user);

                    if (!userUpdated)
                    {
                        // If we have a previous email value, rollback Auth0 email to the previous one to keep it in sync with our database's value 
                        if (!string.IsNullOrWhiteSpace(previousEmail))
                        {
                            ServiceResult auth0UpdateResult = await _auth0Service.UpdateAuth0UserEmail(user.Auth0Id.Auth0UserId, previousEmail);
                        }
                        return ServiceResult<UpdateUserDemographicsResponse>.FailureResult("User database update failed, Auth0 changes rolled back.");
                    }
                }

                UpdateUserDemographicsResponse userResponse = new UpdateUserDemographicsResponse() 
                {
                    FirstName = user.Name.FirstName,
                    LastName = user.Name.LastName,
                    EmailAddress = user.Email.EmailAddress
                };

                return ServiceResult<UpdateUserDemographicsResponse>.SuccessResult(userResponse);
            }
            catch (Exception ex)
            {
                return ServiceResult<UpdateUserDemographicsResponse>.FailureResult("Exception thrown, user update failed", ex);
            }
        }

        public async Task<ServiceResult> MarkUserAsDeleted(Guid uuid)
        {
            try
            {
                bool userSetDeleted = await _userRepository.MarkUserAsDeleted(uuid);

                if (userSetDeleted)
                {
                    return ServiceResult.SuccessResult();
                }

                return ServiceResult.FailureResult("Failed to delete user.");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult("Exception thrown, failed to delete user", ex);
            }
        }
    }
}