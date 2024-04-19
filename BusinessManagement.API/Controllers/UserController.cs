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
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;
        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger; 
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
                    _logger.LogWarning("{trace} New user signup failed", LogHelper.TraceLog());
                    return BadRequest(response.Message ?? "New user signup failed");
                }

                return CreatedAtAction(nameof(NewUserSignup), response.Data );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{trace} Exception thrown", LogHelper.TraceLog());
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [Authorize]
        [HttpGet("{uuid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUser(Guid uuid)
        {
            try
            {
                var response = await _userService.GetUser(uuid);

                if (response == null || !response.Success)
                {
                    _logger.LogWarning("{trace} get user failed", LogHelper.TraceLog());
                    return BadRequest(response?.ErrorMessage);
                }

                return Ok(response.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{trace} Exception thrown", LogHelper.TraceLog());
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [Authorize]
        [HttpPatch("update-user")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateUserDemographics(UpdateUserDemographicsRequest request)
        {
            try
            {
                var response = await _userService.UpdateUserDemographics(request);

                if (response == null || !response.Success)
                {
                    _logger.LogWarning("{trace} update user failed", LogHelper.TraceLog());
                    return BadRequest(response?.ErrorMessage);
                }

                return Ok(response.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{trace} Exception thrown", LogHelper.TraceLog());
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [Authorize]
        [HttpPatch("delete-user/{uuid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> MarkUserAsDeleted([FromRoute] Guid uuid)
        {
            try
            {
                var response = await _userService.MarkUserAsDeleted(uuid);

                if (response == null || !response.Success)
                {
                    _logger.LogWarning("{trace} mark user deleted failed", LogHelper.TraceLog());
                    return BadRequest(response?.ErrorMessage);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{trace} Exception thrown", LogHelper.TraceLog());
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
