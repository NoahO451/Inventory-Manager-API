﻿using App.Models;
using App.Models.DTO;
using App.Models.DTO.Mappers;
using App.Models.DTO.Requests;
using App.Models.DTO.Responses;
using App.Repositories;
using Azure;

namespace App.Services
{
    public interface IInventoryService
    {
        Task<GetInventoryItemResponse> GetInventoryItem(Guid uuid);
        Task<List<GetAllInventoryItemsResponse>> GetAllInventoryItems(Guid userId, Guid businessId);
        /// <summary>
        /// Insert a single inventory item into the database.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<ApiResponse<Guid>> AddInventoryItem(AddInventoryItemRequest request);
        Task<ServiceResult<bool>> RemovedItemResults(Guid uuid);
        Task<ServiceResult<bool>> UpdatedItemResults(UpdatedInventoryItemRequest inventoryItem);
    }

    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _inventoryRepository;
        private readonly ILogger<InventoryService> _logger;

        public InventoryService(IInventoryRepository inventoryRepository, ILogger<InventoryService> logger)
        {
            _inventoryRepository = inventoryRepository;
            _logger = logger;
        }

        public async Task<GetInventoryItemResponse> GetInventoryItem(Guid uuid)
        {
            InventoryItem item = await _inventoryRepository.GetInventoryItem(uuid);

            if (item == null)
            {
                _logger.LogWarning("{trace} item was null", LogHelper.TraceLog());
                return new GetInventoryItemResponse();
            }

            var response = new GetInventoryItemResponse
            {
                InventoryItemUuid = item.InventoryItemUuid,
                PurchaseDate = item.PurchaseDate,
                ReorderQuantity = item.ReorderQuantity,
                Location = item.Location,
                CustomPackageUuid = item.CustomPackageUuid,
                IsListed = item.IsListed,
                IsLot = item.IsLot,
                Notes = item.Notes,

                Name = item.Item.Name,
                Description = item.Item.Description,
                Cost = item.Item.Cost,
                Quantity = item.Item.Quantity,
                ExpirationDate = item.Item.ExpirationDate,
                Category = item.Item.Category,
                ItemWeightG = item.Item.ItemWeightG,

                SKU = item.ItemDetail.SKU,
                SerialNumber = item.ItemDetail.SerialNumber,
                Supplier = item.ItemDetail.Supplier,
                Brand = item.ItemDetail.Brand,
                Model = item.ItemDetail.Model
            };

            return response;
        }

        public async Task<List<GetAllInventoryItemsResponse>> GetAllInventoryItems(Guid userId, Guid businessId)
        {

            List<InventoryItem> inventoryItems = await _inventoryRepository.RetrieveAllInventoryItems(businessId);

            List<GetAllInventoryItemsResponse> response = new List<GetAllInventoryItemsResponse>();

            if (inventoryItems == null || inventoryItems.Count == 0)
            {
                _logger.LogWarning("{trace} inventoryItems null or empty", LogHelper.TraceLog());
                return response;
            }

            foreach (InventoryItem item in inventoryItems)
            {
                response.Add(new GetAllInventoryItemsResponse
                {
                    InventoryItemUuid = item.InventoryItemUuid,
                    Name = item.Item.Name,
                    Description = item.Item.Description,
                    SKU = item.ItemDetail.SKU,
                    Cost = item.Item.Cost,
                    SerialNumber = item.ItemDetail.SerialNumber,
                    PurchaseDate = item.PurchaseDate,
                    Supplier = item.ItemDetail.Supplier,
                    Brand = item.ItemDetail.Brand,
                    Model = item.ItemDetail.Model,
                    Quantity = item.Item.Quantity,
                    ReorderQuantity = item.ReorderQuantity,
                    Location = item.Location,
                    ExpirationDate = item.Item.ExpirationDate,
                    Category = item.Item.Category,
                    CustomPackageUuid = item.CustomPackageUuid,
                    ItemWeightG = item.Item.ItemWeightG,
                    IsListed = item.IsListed,
                    IsLot = item.IsLot,
                    Notes = item.Notes
                });
            }

            return response;
        }

        /// <summary>
        /// Insert a single inventory item into the database.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ApiResponse<Guid>> AddInventoryItem(AddInventoryItemRequest request)
        {
            try
            {
                InventoryItem inventoryItem = InventoryItemMapper.FromRequest(request);

                await _inventoryRepository.CreateInventoryItem(inventoryItem, request.BusinessUuid);

                ApiResponse<Guid> apiResponse = new ApiResponse<Guid>() { Data = inventoryItem.InventoryItemUuid };

                if (await _inventoryRepository.GetInventoryItem(inventoryItem.InventoryItemUuid) != null)
                {
                    apiResponse.Success = true;
                    return apiResponse;
                }

                _logger.LogWarning("{trace} inventoryItems null or empty", LogHelper.TraceLog());
                apiResponse.Success = false;
                return apiResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{trace} Exception thrown", LogHelper.TraceLog());
                return new ApiResponse<Guid>() { Success = false };
            }
        }

        public async Task<ServiceResult<bool>> RemovedItemResults(Guid uuid)
        {
            try
            {
                bool itemDeleted = await _inventoryRepository.RemoveInventoryItem(uuid);

                if (itemDeleted)
                {
                    return ServiceResult<bool>.SuccessResult();
                }

                return ServiceResult<bool>.FailureResult("Failed to delete item.");

            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.FailureResult("Exception thrown, failed to delete item", ex);
            }
        }

        public async Task<ServiceResult<bool>> UpdatedItemResults(UpdatedInventoryItemRequest request)
        {
            try
            {
                InventoryItem inventoryItem = InventoryItemMapper.FromRequest(request);

                bool itemUpdated = await _inventoryRepository.UpdateInventoryItem(inventoryItem);

                if (itemUpdated)
                {
                    return ServiceResult<bool>.SuccessResult();
                }

                return ServiceResult<bool>.FailureResult("Failed to update item.");

            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.FailureResult("Exception thrown, failed to update item", ex);
            }
        }
    }
}
