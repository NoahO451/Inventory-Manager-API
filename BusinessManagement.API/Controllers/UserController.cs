using App.Models;
using App.Models.DTO.Requests;
using App.Models.DTO.Responses;
using App.Services;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("new-user-signup")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> NewUserSignup(NewUserSignupRequest request)
        {
            try
            {
                ApiResponse<NewUserSignupResponse> response = await _userService.NewUserSignup(request);

                if (response == null || !response.Success)
                {
                    return BadRequest(response.Message ?? "New user signup failed");
                }

                return CreatedAtAction(nameof(NewUserSignup), response.Data );
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("{uuid}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUser(Guid uuid)
        {
            try
            {
                ApiResponse<GetUserResponse> response = await _userService.GetUser(uuid);

                if (response == null || !response.Success)
                {
                    return BadRequest(response.Message);
                }

                return Ok(response.Data);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
