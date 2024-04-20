using App.Helpers;
using App.Models;
using App.Models.DTO.Requests;
using App.Models.DTO.Responses;
using App.Models.ValueObjects;
using Azure.Core;
using Dapper;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace App.Repositories
{
    public interface IInventoryRepository
    {
        Task<InventoryItem> GetInventoryItem(Guid uuid);
        Task<List<InventoryItem>> RetrieveAllInventoryItems(Guid businessId);
        Task CreateInventoryItem(InventoryItem request, Guid businessUuid);
        Task<bool> RemoveInventoryItem(Guid uuid);
        Task<bool> UpdateInventoryItem(InventoryItem request);
    }

    public class InventoryRepository : IInventoryRepository
    {
        private DataContext _context;
        private ILogger<InventoryRepository> _logger;

        public InventoryRepository(DataContext context, ILogger<InventoryRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Returns inventory item with matching uuid
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns></returns>
        public async Task<InventoryItem> GetInventoryItem(Guid uuid)
        {
            IEnumerable<InventoryItem>? result = null;

            using (var connection = _context.CreateConnection())
            {
                try
                {
                    connection.Open();

                    string sql = """
                            SELECT 
                            -- Inventory item NON-nested props:
                                ii.inventory_item_uuid AS InventoryItemUuid,
                                ii.purchase_date AS PurchaseDate,
                                ii.reorder_quantity AS ReorderQuantity,
                                ii.location,
                                ii.custom_package_uuid AS CustomPackageUuid,
                                ii.is_listed AS IsListed,
                                ii.is_lot AS IsLOt,
                                ii.notes,

                            -- Item props:
                                ii.name,
                                ii.description,
                                ii.cost,
                                ii.quantity,
                                ii.expiration_date AS ExpirationDate,
                                ii.category,
                                ii.item_weight_g AS ItemWeightG,

                            -- ItemDetail props
                                ii.sku,
                                ii.serial_number AS SerialNumber,
                                ii.supplier,
                                ii.brand,
                                ii.model
                        FROM 
                            inventory_item ii
                        WHERE ii.inventory_item_uuid = @Uuid;
                        """;

                    result = await connection.QueryAsync<InventoryItem, Item, ItemDetail, InventoryItem>(
                        sql,
                        (invItem, item, itemDetail) =>
                        {
                            invItem.Item = item;
                            invItem.ItemDetail = itemDetail;
                            return invItem;
                        },
                        new { Uuid = uuid },
                        splitOn: "name,sku"
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{trace} Exception thrown", LogHelper.TraceLog());
                    return result.FirstOrDefault();
                }
            }
            return result.FirstOrDefault();
        }

        /// <summary>
        /// Returns all inventory items that belong a specific business
        /// </summary>
        /// <param name="businessUuid"></param>
        /// <returns></returns>
        public async Task<List<InventoryItem>> RetrieveAllInventoryItems(Guid businessUuid)
        {
            IEnumerable<InventoryItem>? result = null;

            using (var connection = _context.CreateConnection())
            {
                try
                {
                    connection.Open();

                    string getBusinessIdSQL = """
                        SELECT business_id 
                        FROM business b
                        WHERE b.business_uuid = @BusinessUuid
                        """;

                    int businessId = await connection.QueryFirstOrDefaultAsync<int>(getBusinessIdSQL, new { BusinessUuid = businessUuid });

                    string sql = """
                        SELECT 
                        ii.inventory_item_uuid AS InventoryItemUuid,
                        ii.purchase_date AS PurchaseDate,
                        ii.reorder_quantity AS ReorderQuantity,
                        ii.location,
                        ii.custom_package_uuid AS CustomPackageUuid,
                        ii.is_listed AS IsListed,
                        ii.is_lot AS IsLOt,
                        ii.notes,
                    
                        ii.name,
                        ii.description,
                        ii.cost,
                        ii.quantity,
                        ii.expiration_date AS ExpirationDate,
                        ii.category,
                        ii.item_weight_g AS ItemWeightG,
                    
                        ii.sku,
                        ii.serial_number AS SerialNumber,
                        ii.supplier,
                        ii.brand,
                        ii.model,

                        b.business_id AS BusinessID,
                        b.business_uuid AS BusinessUuid,
                        b.business_name AS BusinessName 
                    FROM 
                        inventory_item ii
                    LEFT JOIN 
                        business_inventory_item bii ON ii.inventory_item_id = bii.inventory_item_id
                    LEFT JOIN 
                        business b ON bii.business_id = b.business_id
                    WHERE bii.business_id = @BusinessId;
                    """;

                    result = await connection.QueryAsync<InventoryItem, Item, ItemDetail, InventoryItem>(
                        sql,
                        (invItem, item, itemDetail) =>
                        {
                            invItem.Item = item;
                            invItem.ItemDetail = itemDetail;
                            return invItem;
                        },
                        new { BusinessId = businessId },
                        splitOn: "name,sku"
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{trace} Exception thrown", LogHelper.TraceLog());
                    return result.ToList();
                }
            }

            return result.ToList();
        }

        /// <summary>
        /// Creates one inventory item and associates it with a specific business
        /// </summary>
        /// <param name="inventoryItem"></param>
        /// <param name="businessUuid"></param>
        /// <returns></returns>
        public async Task CreateInventoryItem(InventoryItem inventoryItem, Guid businessUuid)
        {
            try
            {
                using (var connection = _context.CreateConnection())
                {
                    connection.Open();

                    using (var transaction = connection.BeginTransaction())
                    {
                        string insertInventoryItemSql = """
                        INSERT INTO inventory_item (inventory_item_uuid, name, description, sku, cost, serial_number, purchase_date, supplier, brand, model, quantity, reorder_quantity, location, expiration_date, category, custom_package_uuid, item_weight_g, is_listed, is_lot, notes)
                        VALUES (@InventoryItemUuid, @Name, @Description, @Sku, @Cost, @SerialNumber, @PurchaseDate, @Supplier, @Brand, @Model, @Quantity, @ReorderQuantity, @Location, @ExpirationDate, @Category, @CustomPackageUuid, @ItemWeightG, @IsListed, @IsLot, @Notes)
                        RETURNING inventory_item_id; 
                        """;

                        var parameters = InventoryItemFlatten(inventoryItem);

                        long inventoryItemId = await connection.QueryFirstOrDefaultAsync<long>(insertInventoryItemSql, parameters);

                        string getBusinessIdSQL = """
                        SELECT business_id 
                        FROM business b
                        WHERE b.business_uuid = @BusinessUuid
                        """;

                        int businessId = await connection.QueryFirstOrDefaultAsync<int>(getBusinessIdSQL, new { BusinessUuid = businessUuid });

                        string insertBusinessInventoryItemSql = """
                        INSERT INTO business_inventory_item (inventory_item_id, business_id)
                        VALUES (@InventoryItemId, @BusinessId);
                        """;

                        await connection.ExecuteAsync(insertBusinessInventoryItemSql, new { InventoryItemId = inventoryItemId, BusinessId = businessId });

                        transaction.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{trace} Exception thrown", LogHelper.TraceLog());
            }
        }
        /// <summary>
        /// Removes one inventory item and returns true or false if the item was removed
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns></returns>
        public async Task<bool> RemoveInventoryItem(Guid uuid)
        {
            using (var connection = _context.CreateConnection())
            {
                string removeInventoryItemSql = """
                    DELETE FROM inventory_item ii WHERE ii.inventory_item_uuid = @SelectedIIUuid
                    """;

                var rowsRemoved = await connection.ExecuteAsync(removeInventoryItemSql, param: new { SelectedIIUuid = uuid });

                if (rowsRemoved > 0) { return true; } else { return false; }
            }
        }

        public async Task<bool> UpdateInventoryItem(InventoryItem inventoryItem)
        {
            using (var connection = _context.CreateConnection())
            {
                string updateIISql = """
                    UPDATE 
                        inventory_item
                    SET 
                        name = @Name,
                        description = @Description,
                        sku = @Sku,
                        cost = @Cost,
                        serial_number = @SerialNumber,
                        purchase_date = @PurchaseDate,
                        supplier = @Supplier,
                        brand = @Brand,
                        model = @Model,
                        quantity = @Quantity,
                        reorder_quantity = @ReorderQuantity,
                        location = @Location,
                        expiration_date = @ExpirationDate,
                        category = @Category,
                        custom_package_uuid = @CustomPackageUuid,
                        item_weight_g = @ItemWeightG,
                        is_listed = @IsListed,
                        is_lot = @IsLot,
                        notes = @Notes
                    WHERE
                        inventory_item_uuid = @InventoryItemUuid
                    """;

                var parameters = InventoryItemFlatten(inventoryItem); 

                var rowsUpdated = await connection.ExecuteAsync(updateIISql, parameters);

                if (rowsUpdated > 0) 
                { 
                    return true; 
                } 

                return false;
            }
        }

        /// <summary>
        /// Returns a flattened anonymous object from the InventoryItem type.
        /// </summary>
        /// <param name="inventoryItem"></param>
        /// <returns></returns>
        private object InventoryItemFlatten(InventoryItem inventoryItem)
        {
            return new
            {
                InventoryItemUuid = inventoryItem.InventoryItemUuid,
                Name = inventoryItem.Item.Name,
                Description = inventoryItem.Item.Description,
                Sku = inventoryItem.ItemDetail.SKU,
                Cost = inventoryItem.Item.Cost,
                SerialNumber = inventoryItem.ItemDetail.SerialNumber,
                PurchaseDate = inventoryItem.PurchaseDate,
                Supplier = inventoryItem.ItemDetail.Supplier,
                Brand = inventoryItem.ItemDetail.Brand,
                Model = inventoryItem.ItemDetail.Model,
                Quantity = inventoryItem.Item.Quantity,
                ReorderQuantity = inventoryItem.ReorderQuantity,
                Location = inventoryItem.Location,
                ExpirationDate = inventoryItem.Item.ExpirationDate,
                Category = inventoryItem.Item.Category,
                CustomPackageUuid = inventoryItem.CustomPackageUuid,
                ItemWeightG = inventoryItem.Item.ItemWeightG,
                IsListed = inventoryItem.IsListed,
                IsLot = inventoryItem.IsLot,
                Notes = inventoryItem.Notes
            };
        }
    }
}