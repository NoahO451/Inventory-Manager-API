﻿using App.Models.DTO.Requests;
using App.Models.DTO.Responses;
using App.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers
{
    [ApiController]
    [Route("api/inventory-items")]
    [Authorize]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;
        private readonly ILogger<InventoryController> _logger;
        private readonly IValidator<UpdateInventoryItemRequest> _updateInventoryValidator;
        private readonly IValidator<AddInventoryItemRequest> _addInventoryValidator;

        public InventoryController(IInventoryService inventoryService,
            ILogger<InventoryController> logger,
            IValidator<UpdateInventoryItemRequest> updateInventoryValidator,
            IValidator<AddInventoryItemRequest> addInventoryValidator)
        {
            _inventoryService = inventoryService;
            _updateInventoryValidator = updateInventoryValidator;
            _addInventoryValidator = addInventoryValidator;
            _logger = logger;
        }
        /// <summary>
        /// Get inventory item by ID.
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns></returns>
        [HttpGet("{uuid}")]
        [Authorize("get:inventory-item")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetInventoryItem(Guid uuid)
        {
            if (Guid.Empty == uuid)
            {
                _logger.LogWarning("{trace} uuid was empty", LogHelper.TraceLog());
                return BadRequest();
            }

            try
            {
                var result = await _inventoryService.GetInventoryItem(uuid);

                if (result == null || !result.Success)
                {
                   _logger.LogWarning("{trace} InventoryItem was null", LogHelper.TraceLog());
                    return BadRequest();
                }

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{trace} Exception thrown", LogHelper.TraceLog());
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Get all inventory items owned by business.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="businessId"></param>
        /// <returns></returns>
        [HttpGet("user/{userId}/business/{businessId}")]
        [Authorize("get:inventory-item")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllInventoryItems(Guid userId, Guid businessId)
        {
            if (Guid.Empty == userId || Guid.Empty == businessId)
            {
                _logger.LogWarning("{trace} a uuid was empty", LogHelper.TraceLog());
                return BadRequest();
            }

            try
            {
                var result = await _inventoryService.GetAllInventoryItems(userId, businessId);

                if (result == null || !result.Success)
                {
                    _logger.LogWarning("{trace} inventoryItems were null", LogHelper.TraceLog());
                    return BadRequest();
                }

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{trace} Exception thrown", LogHelper.TraceLog());
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Create a new inventory item.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize("create:inventory-item")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddInventoryItem(AddInventoryItemRequest request)
        {
            ValidationResult validationResult = _addInventoryValidator.Validate(request);

            if (!validationResult.IsValid)
            {
                _logger.LogWarning("{trace} validation failed: {err}",
                    LogHelper.TraceLog(), LogHelper.ErrorList(validationResult));

                return BadRequest(validationResult.Errors);
            }

            try
            {
                var result = await _inventoryService.AddInventoryItem(request);

                if (result == null || !result.Success)
                {
                    _logger.LogWarning("{trace} result was null", LogHelper.TraceLog());
                    return BadRequest();
                }

                return CreatedAtAction(nameof(AddInventoryItem), new { id = result.Data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{trace} Exception thrown", LogHelper.TraceLog());
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Delete an inventory item using its ID. 
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns></returns>
        [HttpDelete("{uuid}")]
        [Authorize("delete:inventory-item")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoveInventoryItem(Guid uuid)
        {
            try
            {
                var result = await _inventoryService.RemovedItemResults(uuid);

                if (result == null || !result.Success) 
                {
                    _logger.LogWarning("{trace} result was null", LogHelper.TraceLog());
                    return BadRequest(result?.ErrorMessage);
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{trace} Exception thrown", LogHelper.TraceLog());
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Update an existing inventory item. 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPatch]
        [Authorize("update:inventory-item")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateInventoryItem(UpdateInventoryItemRequest request)
        {
            ValidationResult validationResult = _updateInventoryValidator.Validate(request);

            if (!validationResult.IsValid)
            {
                _logger.LogWarning("{trace} validation failed: {err}",
                    LogHelper.TraceLog(), LogHelper.ErrorList(validationResult)); 

                return BadRequest(validationResult.Errors);
            }

            try
            {
                var result = await _inventoryService.UpdatedItemResults(request);

                if (result == null || !result.Success)
                {
                    _logger.LogWarning("{trace} result was null", LogHelper.TraceLog());
                    return BadRequest(result?.ErrorMessage);
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{trace} Exception thrown", LogHelper.TraceLog());
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}