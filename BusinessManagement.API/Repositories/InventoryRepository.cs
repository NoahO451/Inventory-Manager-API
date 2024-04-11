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
    }

    public class InventoryRepository : IInventoryRepository
    {
        private DataContext _context;

        public InventoryRepository(DataContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns inventory item with matching uuid
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns></returns>
        public async Task<InventoryItem> GetInventoryItem(Guid uuid)
        {
            IEnumerable<InventoryItem> result = null;

            using (var connection = _context.CreateConnection())
            {
                connection.Open();

                // Notice how this query is spaced weird. If you compare it to the InventoryItem's structure, we're returning the database columns
                // in the same order that they appear in the aggregate. 
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

                // Set the QueryAsync types to the three classes we pass in, and the class we expect to end with
                result = await connection.QueryAsync<InventoryItem, Item, ItemDetail, InventoryItem>(
                    sql, // pass in SQL query from above
                    (invItem, item, itemDetail) => // These are the three classes we're dealing with
                    {
                        invItem.Item = item; // error, Item is a private accessor and only available through constructor
                        invItem.ItemDetail = itemDetail;
                        return invItem;
                    },
                    new { Uuid = uuid },
                    // Specify columns to split the result set on. We will palce the value each column from the database result to InventoryItem. Once we 
                    // reach the "ii.name", we put those column's values into the "Item" object. Once we reach "ii.sku", those values go into the ItemDetails object.
                    splitOn: "name,sku" 
                );
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
                connection.Open();

                // Get the business' primary key for a join we'll need to perform 
                string getBusinessIdSQL = """
                        SELECT business_id 
                        FROM business b
                        WHERE b.business_uuid = @BusinessUuid
                        """;

                int businessId = await connection.QueryFirstOrDefaultAsync<int>(getBusinessIdSQL, new { BusinessUuid = businessUuid });

                // Notice how this query is spaced weird. If you compare it to the InventoryItem's structure, we're returning the database columns
                // in the same order that they appear in the aggregate. 
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
                    
                        -- This is where InventoryItem.Item's props start
                        ii.name,
                        ii.description,
                        ii.cost,
                        ii.quantity,
                        ii.expiration_date AS ExpirationDate,
                        ii.category,
                        ii.item_weight_g AS ItemWeightG,
                    
                        -- This is where InventoryItem.ItemDetail's props start
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

                // Set the QueryAsync types to the three classes we pass in, and the class we expect to end with
                result = await connection.QueryAsync<InventoryItem, Item, ItemDetail, InventoryItem>(
                    sql, // pass in SQL query from above
                    (invItem, item, itemDetail) => // These are the three classes we're dealing with
                    {
                        invItem.Item = item;
                        invItem.ItemDetail = itemDetail;
                        return invItem;
                    },
                    new { BusinessId = businessId },
                    // Specify columns to split the result set on. We will palce the value each column from the database result to InventoryItem. Once we 
                    // reach the "ii.name", we put those column's values into the "Item" object. Once we reach "ii.sku", those values go into the ItemDetails object.
                    splitOn: "name,sku" 
                );
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
            using (var connection = _context.CreateConnection())
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    // Basic INSERT query
                    string insertInventoryItemSql = """
                        INSERT INTO inventory_item (inventory_item_uuid, name, description, sku, cost, serial_number, purchase_date, supplier, brand, model, quantity, reorder_quantity, location, expiration_date, category, custom_package_uuid, item_weight_g, is_listed, is_lot, notes)
                        VALUES (@InventoryItemUuid, @Name, @Description, @Sku, @Cost, @SerialNumber, @PurchaseDate, @Supplier, @Brand, @Model, @Quantity, @ReorderQuantity, @Location, @ExpirationDate, @Category, @CustomPackageUuid, @ItemWeightG, @IsListed, @IsLot, @Notes)
                        RETURNING inventory_item_id; 
                        """; // RETURNING returns the primary key so we can use it later in the join table

                    // Since the InventoryItem class is an aggregate of other classes, it's easier to "flatten" it 
                    // out here and send it to the datatbase as one simple object, than it would be to try and make 
                    // Dapper understand the shape of this class. I'm also tired.
                    var parameters = InventoryItemFlatten(inventoryItem);

                    // Flattened invetory item object now fills in the "@" paramters from the query above
                    long inventoryItemId = await connection.QueryFirstOrDefaultAsync<long>(insertInventoryItemSql, parameters);

                    // Use the GUID to get the business' primary key
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

                    // Add the inventory item and business to the join table via their PKs instead of GUIDs. This is the only time that the primary key
                    // should really show up in the API. The PK has not reason to make it to the client.
                    await connection.ExecuteAsync(insertBusinessInventoryItemSql, new { InventoryItemId = inventoryItemId, BusinessId = businessId });

                    transaction.Commit();
                }
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