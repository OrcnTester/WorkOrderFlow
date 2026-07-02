using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WorkOrderFlow.Web.Data;
using WorkOrderFlow.Web.Models;
using WorkOrderFlow.Web.Services;

namespace WorkOrderFlow.Tests;

public class InventoryStockServiceTests
{
    private static ApplicationDbContext CreateContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();

        return context;
    }

    [Fact]
    public async Task ApplyStockMovementAsync_IncreasesStock_AndCreatesTransaction()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        await using var context = CreateContext(connection);

        var item = new InventoryItem
        {
            Name = "Chain Oil",
            Sku = "CHAIN-OIL-001",
            QuantityOnHand = 5,
            ReorderLevel = 2,
            UnitCost = 100,
            SalePrice = 150
        };

        context.InventoryItems.Add(item);
        await context.SaveChangesAsync();

        var service = new InventoryStockService(context);

        var success = await service.ApplyStockMovementAsync(
            item.Id,
            3,
            InventoryTransactionType.ManualAdjustment,
            "Test stock increase");

        var updatedItem = await context.InventoryItems.FirstAsync(i => i.Id == item.Id);
        var transaction = await context.InventoryTransactions.FirstAsync();

        Assert.True(success);
        Assert.Equal(8, updatedItem.QuantityOnHand);
        Assert.Equal(InventoryTransactionType.ManualAdjustment, transaction.Type);
        Assert.Equal(3, transaction.QuantityChange);
        Assert.Equal(8, transaction.QuantityAfter);
        Assert.Equal("Test stock increase", transaction.Notes);
    }

    [Fact]
    public async Task ApplyStockMovementAsync_DecreasesStock_AndCreatesTransaction()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        await using var context = CreateContext(connection);

        var item = new InventoryItem
        {
            Name = "Brake Pad",
            Sku = "BRAKE-PAD-001",
            QuantityOnHand = 10,
            ReorderLevel = 2,
            UnitCost = 250,
            SalePrice = 350
        };

        context.InventoryItems.Add(item);
        await context.SaveChangesAsync();

        var service = new InventoryStockService(context);

        var success = await service.ApplyStockMovementAsync(
            item.Id,
            -4,
            InventoryTransactionType.WorkOrderUsage,
            "Material used on test work order",
            workOrderId: null);

        var updatedItem = await context.InventoryItems.FirstAsync(i => i.Id == item.Id);
        var transaction = await context.InventoryTransactions.FirstAsync();

        Assert.True(success);
        Assert.Equal(6, updatedItem.QuantityOnHand);
        Assert.Equal(InventoryTransactionType.WorkOrderUsage, transaction.Type);
        Assert.Equal(-4, transaction.QuantityChange);
        Assert.Equal(6, transaction.QuantityAfter);
    }

    [Fact]
    public async Task ApplyStockMovementAsync_DoesNotAllowNegativeStock()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        await using var context = CreateContext(connection);

        var item = new InventoryItem
        {
            Name = "Low Stock Item",
            Sku = "LOW-001",
            QuantityOnHand = 2,
            ReorderLevel = 1,
            UnitCost = 50,
            SalePrice = 75
        };

        context.InventoryItems.Add(item);
        await context.SaveChangesAsync();

        var service = new InventoryStockService(context);

        var success = await service.ApplyStockMovementAsync(
            item.Id,
            -5,
            InventoryTransactionType.WorkOrderUsage,
            "Should fail");

        var updatedItem = await context.InventoryItems.FirstAsync(i => i.Id == item.Id);
        var transactionCount = await context.InventoryTransactions.CountAsync();

        Assert.False(success);
        Assert.Equal(2, updatedItem.QuantityOnHand);
        Assert.Equal(0, transactionCount);
    }

    [Fact]
    public async Task ApplyStockMovementAsync_ReturnsFalse_WhenInventoryItemDoesNotExist()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        await using var context = CreateContext(connection);

        var service = new InventoryStockService(context);

        var success = await service.ApplyStockMovementAsync(
            999,
            5,
            InventoryTransactionType.ManualAdjustment,
            "Missing item");

        Assert.False(success);
    }
}