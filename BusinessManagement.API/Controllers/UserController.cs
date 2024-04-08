using App.Models;
using App.Models.DTO.Requests;
using App.Models.DTO.Responses;
using App.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }
        public async Task<IActionResult> NewUserSignup(NewUserSignupRequest request)
        {
            try
            {
                ApiResponse<NewUserSignupResponse> response = await _userService.NewUserSignup(request);

                return Ok();
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
