using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WorkOrderFlow.Web.Data;
using WorkOrderFlow.Web.Models;
using WorkOrderFlow.Web.Services;

namespace WorkOrderFlow.Tests;

public class WorkOrderWorkflowServiceTests
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
    public async Task ChangeStatusAsync_ChangesStatus_AndCreatesHistoryRecord()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        await using var context = CreateContext(connection);

        var customer = new Customer
        {
            FullName = "Test Customer",
            Phone = "5550000000"
        };

        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        var workOrder = new WorkOrder
        {
            CustomerId = customer.Id,
            Title = "Test Work Order",
            Status = WorkOrderStatus.New,
            Priority = WorkOrderPriority.Medium,
            CreatedAt = DateTime.UtcNow
        };

        context.WorkOrders.Add(workOrder);
        await context.SaveChangesAsync();

        var service = new WorkOrderWorkflowService(context);

        var success = await service.ChangeStatusAsync(
            workOrder.Id,
            WorkOrderStatus.InProgress,
            "Work started",
            clearCompletedAt: true);

        var updatedWorkOrder = await context.WorkOrders.FirstAsync(w => w.Id == workOrder.Id);
        var history = await context.WorkOrderStatusHistories.FirstAsync();

        Assert.True(success);
        Assert.Equal(WorkOrderStatus.InProgress, updatedWorkOrder.Status);
        Assert.Null(updatedWorkOrder.CompletedAt);
        Assert.Equal(WorkOrderStatus.New, history.FromStatus);
        Assert.Equal(WorkOrderStatus.InProgress, history.ToStatus);
        Assert.Equal("Work started", history.Notes);
    }

    [Fact]
    public async Task ChangeStatusAsync_SetsCompletedAt_WhenWorkIsCompleted()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        await using var context = CreateContext(connection);

        var customer = new Customer
        {
            FullName = "Test Customer",
            Phone = "5550000000"
        };

        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        var workOrder = new WorkOrder
        {
            CustomerId = customer.Id,
            Title = "Complete Test",
            Status = WorkOrderStatus.InProgress,
            Priority = WorkOrderPriority.High,
            CreatedAt = DateTime.UtcNow
        };

        context.WorkOrders.Add(workOrder);
        await context.SaveChangesAsync();

        var service = new WorkOrderWorkflowService(context);

        var success = await service.ChangeStatusAsync(
            workOrder.Id,
            WorkOrderStatus.Completed,
            "Work completed",
            setCompletedAt: true);

        var updatedWorkOrder = await context.WorkOrders.FirstAsync(w => w.Id == workOrder.Id);
        var history = await context.WorkOrderStatusHistories.FirstAsync();

        Assert.True(success);
        Assert.Equal(WorkOrderStatus.Completed, updatedWorkOrder.Status);
        Assert.NotNull(updatedWorkOrder.CompletedAt);
        Assert.Equal(WorkOrderStatus.InProgress, history.FromStatus);
        Assert.Equal(WorkOrderStatus.Completed, history.ToStatus);
    }

    [Fact]
    public async Task ChangeStatusAsync_ClearsCompletedAt_WhenWorkIsReopened()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        await using var context = CreateContext(connection);

        var customer = new Customer
        {
            FullName = "Test Customer",
            Phone = "5550000000"
        };

        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        var workOrder = new WorkOrder
        {
            CustomerId = customer.Id,
            Title = "Reopen Test",
            Status = WorkOrderStatus.Completed,
            Priority = WorkOrderPriority.Medium,
            CompletedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        context.WorkOrders.Add(workOrder);
        await context.SaveChangesAsync();

        var service = new WorkOrderWorkflowService(context);

        var success = await service.ChangeStatusAsync(
            workOrder.Id,
            WorkOrderStatus.InProgress,
            "Work reopened",
            clearCompletedAt: true);

        var updatedWorkOrder = await context.WorkOrders.FirstAsync(w => w.Id == workOrder.Id);
        var history = await context.WorkOrderStatusHistories.FirstAsync();

        Assert.True(success);
        Assert.Equal(WorkOrderStatus.InProgress, updatedWorkOrder.Status);
        Assert.Null(updatedWorkOrder.CompletedAt);
        Assert.Equal(WorkOrderStatus.Completed, history.FromStatus);
        Assert.Equal(WorkOrderStatus.InProgress, history.ToStatus);
    }

    [Fact]
    public async Task ChangeStatusAsync_ReturnsFalse_WhenWorkOrderDoesNotExist()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        await using var context = CreateContext(connection);

        var service = new WorkOrderWorkflowService(context);

        var success = await service.ChangeStatusAsync(
            999,
            WorkOrderStatus.InProgress,
            "Missing work order");

        Assert.False(success);
    }
}