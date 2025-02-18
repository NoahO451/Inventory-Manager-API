﻿using App.Models;
using App.Models.DTO.Requests;
using App.Models.DTO.Responses;
using App.Repositories;
using App.Services;
using FluentValidation;
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
        private readonly IValidator<CreateNewBusinessRequest> _newBusinessValidator;
        private readonly IValidator<UpdateBusinessInformationRequest> _updatedBusinessInfoValidator;
        public BusinessController(IBusinessService businessService, 
            ILogger<BusinessController> logger, 
            IValidator<CreateNewBusinessRequest> newBusinessValidator,
            IValidator<UpdateBusinessInformationRequest> updatedBusinessInfoValidator)
        {
            _businessService = businessService;
            _newBusinessValidator = newBusinessValidator;
            _updatedBusinessInfoValidator = updatedBusinessInfoValidator;
            _logger = logger;
        }
        /// <summary>
        /// Create a new business.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize("create:business")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateNewBusiness(CreateNewBusinessRequest request)
        {
            ValidationResult validationResult = _newBusinessValidator.Validate(request);

            if (!validationResult.IsValid)
            {
                _logger.LogWarning("{trace} validation failed: {err}",
                     LogHelper.TraceLog(), LogHelper.ErrorList(validationResult));

                return BadRequest(validationResult.Errors);
            }

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
        [HttpDelete("{uuid}")]
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
        [Authorize("get:business")]
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

        /// <summary>
        /// Update an existing business with new information.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPatch]
        [Authorize("update:business")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateBusinessInformation(UpdateBusinessInformationRequest request)
        {
            ValidationResult validationResult = _updatedBusinessInfoValidator.Validate(request);

            if (!validationResult.IsValid)
            {
                _logger.LogWarning("{trace} validation failed: {err}",
                     LogHelper.TraceLog(), LogHelper.ErrorList(validationResult));

                return BadRequest(validationResult.Errors);
            }

            try
            {
                var result = await _businessService.UpdateBusinessInformation(request);

                if (result == null || !result.Success)
                {
                    _logger.LogWarning("{trace} result was null or failed", LogHelper.TraceLog());
                    return BadRequest(result?.ErrorMessage);
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{trace} exception thrown", LogHelper.TraceLog());
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
