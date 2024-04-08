using App.Models;
using App.Models.DTO.Requests;
using App.Models.DTO.Responses;
using App.Repositories;

namespace App.Services
{
    public interface IUserService 
    {
        Task<ApiResponse<NewUserSignupResponse>> NewUserSignup(NewUserSignupRequest request); 
    }

    public class UserService : IUserService
    {
        private IUserRepository _userRepository;
        public UserService(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public Task<ApiResponse<NewUserSignupResponse>> NewUserSignup(NewUserSignupRequest request)
        {
            try
            {
                
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
