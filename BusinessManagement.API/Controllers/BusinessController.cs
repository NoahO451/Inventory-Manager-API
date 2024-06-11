using App.Models;
using App.Models.DTO.Responses;
using App.Repositories;
using App.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers
{
    [ApiController]
    [Route("api/businesses")]
    public class BusinessController : ControllerBase
    {
        private readonly IBusinessService _businessService;
        private readonly ILogger<BusinessController> _logger;
        public BusinessController(IBusinessService businessService, ILogger<BusinessController> logger)
        {
            _businessService = businessService;
            _logger = logger;
        }
        /// <summary>
        /// Create a new business.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("createBusiness")]
        [Authorize("create:new-business")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateNewBusiness(BusinessResponse request)
        {
            try
            {
                var result = await _businessService.CreateNewBusiness(request);

                if (result == null || !result.Success)
                {
                    _logger.LogWarning("{trace} new business creation failed", LogHelper.TraceLog());
                    return BadRequest(result?.ErrorMessage ?? "new business creation failed");
                }

                return CreatedAtAction(nameof(CreateNewBusiness), result.Data);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{trace} exception thrown", LogHelper.TraceLog());
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Get business information using business' ID.
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns></returns>
        [HttpGet("{uuid}")]
        [Authorize("get:business")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBusiness(Guid uuid)
        {
            if (Guid.Empty == uuid)
            {
                _logger.LogWarning("{trace} uuid was empty", LogHelper.TraceLog());
                return BadRequest();
            }

            try
            {
                var result = await _businessService.GetBusiness(uuid);

                if (result == null || !result.Success)
                {
                    _logger.LogWarning("{trace} get business failed", LogHelper.TraceLog());
                    return BadRequest(result?.ErrorMessage);
                }

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{trace} exception thrown", LogHelper.TraceLog());
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Marks business as deleted using its ID.
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns></returns>
        [HttpPatch("{uuid}")]
        [Authorize("delete:business")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> MarkBusinessAsDeleted(Guid uuid)
        {
            if (Guid.Empty == uuid)
            {
                _logger.LogWarning("{trace} uuid was empty", LogHelper.TraceLog());
                return BadRequest();
            }
            try
            {
                var result = await _businessService.MarkBusinessAsDeleted(uuid);

                if (result == null || !result.Success)
                {
                    _logger.LogWarning("{trace} mark business deleted failed", LogHelper.TraceLog());
                    return BadRequest(result?.ErrorMessage);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{trace} exception thrown", LogHelper.TraceLog());
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
        /// <summary>
        /// Get all businesses under the specified user.
        /// </summary>
        /// <param name="userUuid"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize("get:businesses")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllBusinesses(Guid userUuid)
        {
            if (Guid.Empty == userUuid)
            {
                _logger.LogWarning("{trace} uuid was empty", LogHelper.TraceLog());
                return BadRequest();
            }

            try
            {
                var result = await _businessService.GetAllBusnesses(userUuid);

                if (result == null || !result.Success)
                {
                    _logger.LogWarning("{trace} businesses were null", LogHelper.TraceLog());
                    return BadRequest();
                }

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{trace} exception thrown", LogHelper.TraceLog());
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
