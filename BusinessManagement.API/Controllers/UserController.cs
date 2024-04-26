using App.Models;
using App.Models.DTO.Requests;
using App.Models.DTO.Responses;
using App.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;
        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger; 
        }

        [HttpPost("signup")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> NewUserSignup(NewUserSignupRequest request)
        {
            try
            {
                var result = await _userService.NewUserSignup(request);

                if (result == null || !result.Success)
                {
                    _logger.LogWarning("{trace} New user signup failed", LogHelper.TraceLog());
                    return BadRequest(result?.ErrorMessage ?? "New user signup failed");
                }

                return CreatedAtAction(nameof(NewUserSignup), result.Data );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{trace} Exception thrown", LogHelper.TraceLog());
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("{uuid}")]
        [Authorize("get:user")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUser(Guid uuid)
        {
            if (Guid.Empty == uuid)
            {
                _logger.LogWarning("{trace} uuid was empty", LogHelper.TraceLog());
                return BadRequest();
            }

            try
            {
                var result = await _userService.GetUser(uuid);

                if (result == null || !result.Success)
                {
                    _logger.LogWarning("{trace} get user failed", LogHelper.TraceLog());
                    return BadRequest(result?.ErrorMessage);
                }

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{trace} Exception thrown", LogHelper.TraceLog());
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPatch]
        [Authorize("update:user")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateUserDemographics(UpdateUserDemographicsRequest request)
        {
            try
            {
                var result = await _userService.UpdateUserDemographics(request);

                if (result == null || !result.Success)
                {
                    _logger.LogWarning("{trace} result was null or failed", LogHelper.TraceLog());
                    return BadRequest(result?.ErrorMessage);
                }

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{trace} Exception thrown", LogHelper.TraceLog());
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPatch("{uuid}")]
        [Authorize("delete:user")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> MarkUserAsDeleted(Guid uuid)
        {
            if (Guid.Empty == uuid)
            {
                _logger.LogWarning("{trace} uuid was empty", LogHelper.TraceLog());
                return BadRequest();
            }

            try
            {
                var result = await _userService.MarkUserAsDeleted(uuid);

                if (result == null || !result.Success)
                {
                    _logger.LogWarning("{trace} mark user deleted failed", LogHelper.TraceLog());
                    return BadRequest(result?.ErrorMessage);
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