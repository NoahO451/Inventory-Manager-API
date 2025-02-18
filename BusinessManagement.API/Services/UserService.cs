﻿using App.Models;
using App.Models.DTO;
using App.Models.DTO.Requests;
using App.Models.DTO.Responses;
using App.Models.ValueObjects;
using App.Repositories;

namespace App.Services
{
    public interface IUserService
    {
        Task<ServiceResult<NewUserSignupResponse>> NewUserSignup(NewUserSignupRequest request);
        Task<ServiceResult<GetUserResponse>> GetUser(Guid uuid);
        Task<ServiceResult<UpdateUserDemographicsResponse>> UpdateUserDemographics(UpdateUserDemographicsRequest request);
        Task<ServiceResult> MarkUserAsDeleted(Guid uuid);
    }

    public class UserService : IUserService
    {
        private IUserRepository _userRepository;

        private IAuth0Service _auth0Service;

        private ILogger<UserService> _logger;
        public UserService(IUserRepository userRepository, IAuth0Service auth0Service, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _auth0Service = auth0Service;
            _logger = logger;
        }

        public async Task<ServiceResult<NewUserSignupResponse>> NewUserSignup(NewUserSignupRequest req)
        {
            try
            {
                Guid NewUserUuid = Guid.NewGuid();

                UserData newUser = new UserData(
                    NewUserUuid,
                    new Auth0Id(req.FullAuth0Id),
                    new Name(req.FullName, req.Nickname),
                    new Email(req.Email),
                    null,
                    DateTime.UtcNow,
                    DateTime.UtcNow,
                    false,
                    false
                );

                bool userCreated = await _userRepository.CreateNewUser(newUser);

                if (!userCreated)
                {
                    _logger.LogWarning("{trace} Failed to create new user", LogHelper.TraceLog());
                    return ServiceResult<NewUserSignupResponse>.FailureResult("There was an error saving to the database.");
                }

                var newUserResponse = new NewUserSignupResponse()
                {
                    UserUuid = newUser.UserUuid,
                    FullName = newUser.Name.FullName,
                    FirstName = newUser.Name.FirstName,
                    LastName = newUser.Name.LastName,
                    Nickname = newUser.Name.Nickname,
                    Email = newUser.Email.EmailAddress,
                    IsPremiumMember = newUser.IsPremiumMember
                };

                return ServiceResult<NewUserSignupResponse>.SuccessResult(newUserResponse);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{trace} Exception thrown", LogHelper.TraceLog());
                throw;
            }
        }

        public async Task<ServiceResult<GetUserResponse>> GetUser(Guid uuid)
        {
            UserData? user = await _userRepository.GetUserByUuid(uuid);

            if (user == null || user.IsDeleted)
            {
                _logger.LogWarning("{trace} User null or deleted", LogHelper.TraceLog());
                return ServiceResult<GetUserResponse>.FailureResult("Error getting the user.");
            }

            var userResponse = new GetUserResponse()
            {
                UserUuid = user.UserUuid,
                Auth0UserId = user.Auth0Id.Auth0UserId,
                FirstName = user.Name.FirstName, 
                LastName = user.Name.LastName,
                Nickname = user.Name.Nickname,
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

                string? fullName = string.IsNullOrWhiteSpace(req.FullName) ? user.Name.FullName : req.FullName;
                string? nickName = string.IsNullOrWhiteSpace(req.Nickname) ? user.Name.Nickname : req.Nickname;

                if (fullName != user.Name.FullName || nickName != user.Name.Nickname)
                {
                    Name newName = new Name(fullName, nickName);

                    user.SetName(newName);
                    updateRequired = true;
                }

                if (user.Email.EmailAddress != req.EmailAddress)
                {
                    ServiceResult auth0UpdateResult = await _auth0Service.UpdateAuth0UserEmail(user.Auth0Id.Auth0UserId, req.EmailAddress);

                    if (!auth0UpdateResult.Success)
                    {
                        return ServiceResult<UpdateUserDemographicsResponse>.FailureResult(auth0UpdateResult.ErrorMessage ?? "Failed to update Auth0 email.");
                    }

                    Email newEmail = new Email(req.EmailAddress);

                    user.SetEmail(newEmail);
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
                            // Retry for rollback
                            bool rollbackSuccess = false;
                            int retryCount = 3; 
                            int retryDelayMs = 1000; 

                            for (int i = 0; i < retryCount; i++)
                            {
                                ServiceResult auth0UpdateResult = await _auth0Service.UpdateAuth0UserEmail(user.Auth0Id.Auth0UserId, previousEmail);

                                if (auth0UpdateResult.Success)
                                {
                                    rollbackSuccess = true;
                                    break; 
                                }
                                else
                                {
                                    _logger.LogWarning("{trace} Retry attempt {attempt}: Auth0 rollback failed", LogHelper.TraceLog(), i + 1);
                                    await Task.Delay(retryDelayMs);
                                }
                            }

                            // If we reach this point, we're in trouble
                            if (!rollbackSuccess)
                            {
                                _logger.LogCritical("{trace} Auth0 rollback failed after {retryCount} retry attempts", LogHelper.TraceLog(), retryCount);
                                return ServiceResult<UpdateUserDemographicsResponse>.FailureResult("User database update failed, Auth0 rollback failed.");
                            }
                        }
                        _logger.LogWarning("{trace} User update failed, Auth0 changes rolled back", LogHelper.TraceLog());
                        return ServiceResult<UpdateUserDemographicsResponse>.FailureResult("User database update failed, Auth0 changes rolled back.");
                    }
                }


                UpdateUserDemographicsResponse userResponse = new UpdateUserDemographicsResponse() 
                {
                    FullName = user.Name.FullName,
                    FirstName = user.Name.FirstName, 
                    LastName = user.Name.LastName,
                    Nickname = user.Name.Nickname,
                    EmailAddress = user.Email.EmailAddress
                };

                return ServiceResult<UpdateUserDemographicsResponse>.SuccessResult(userResponse);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "{trace} Exception thrown", LogHelper.TraceLog());
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

                _logger.LogWarning("{trace} Failed to delete user", LogHelper.TraceLog());
                return ServiceResult.FailureResult("Failed to delete user.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{trace} Exception thrown", LogHelper.TraceLog());
                return ServiceResult.FailureResult("Exception thrown, failed to delete user", ex);
            }
        }
    }
}