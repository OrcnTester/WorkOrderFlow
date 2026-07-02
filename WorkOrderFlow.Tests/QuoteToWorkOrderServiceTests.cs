using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WorkOrderFlow.Web.Data;
using WorkOrderFlow.Web.Models;
using WorkOrderFlow.Web.Services;

namespace WorkOrderFlow.Tests;

public class QuoteToWorkOrderServiceTests
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
    public async Task CreateOrGetWorkOrderFromQuoteAsync_CreatesWorkOrder_AndMarksQuoteAccepted()
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

        var quote = new Quote
        {
            CustomerId = customer.Id,
            Title = "Test Quote",
            LaborCost = 1000,
            PartsCost = 500,
            Discount = 100,
            Status = QuoteStatus.Sent,
            Notes = "Quote notes",
            CreatedAt = DateTime.UtcNow
        };

        context.Quotes.Add(quote);
        await context.SaveChangesAsync();

        var service = new QuoteToWorkOrderService(context);

        var workOrderId = await service.CreateOrGetWorkOrderFromQuoteAsync(quote.Id);

        var updatedQuote = await context.Quotes.FirstAsync(q => q.Id == quote.Id);
        var workOrder = await context.WorkOrders.FirstAsync(w => w.Id == workOrderId);

        Assert.NotNull(workOrderId);
        Assert.Equal(QuoteStatus.Accepted, updatedQuote.Status);
        Assert.Equal(quote.Id, workOrder.QuoteId);
        Assert.Equal(customer.Id, workOrder.CustomerId);
        Assert.Equal("Test Quote", workOrder.Title);
        Assert.Equal("Quote notes", workOrder.Description);
        Assert.Equal(WorkOrderStatus.New, workOrder.Status);
        Assert.Equal(WorkOrderPriority.Medium, workOrder.Priority);
    }

    [Fact]
    public async Task CreateOrGetWorkOrderFromQuoteAsync_DoesNotCreateDuplicateWorkOrder()
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

        var quote = new Quote
        {
            CustomerId = customer.Id,
            Title = "Duplicate Test Quote",
            LaborCost = 1000,
            PartsCost = 250,
            Discount = 0,
            Status = QuoteStatus.Sent,
            CreatedAt = DateTime.UtcNow
        };

        context.Quotes.Add(quote);
        await context.SaveChangesAsync();

        var existingWorkOrder = new WorkOrder
        {
            CustomerId = customer.Id,
            QuoteId = quote.Id,
            Title = quote.Title,
            Status = WorkOrderStatus.New,
            Priority = WorkOrderPriority.Medium,
            CreatedAt = DateTime.UtcNow
        };

        context.WorkOrders.Add(existingWorkOrder);
        await context.SaveChangesAsync();

        var service = new QuoteToWorkOrderService(context);

        var workOrderId = await service.CreateOrGetWorkOrderFromQuoteAsync(quote.Id);

        var workOrderCount = await context.WorkOrders.CountAsync(w => w.QuoteId == quote.Id);
        var updatedQuote = await context.Quotes.FirstAsync(q => q.Id == quote.Id);

        Assert.Equal(existingWorkOrder.Id, workOrderId);
        Assert.Equal(1, workOrderCount);
        Assert.Equal(QuoteStatus.Accepted, updatedQuote.Status);
    }

    [Fact]
    public async Task CreateOrGetWorkOrderFromQuoteAsync_ReturnsNull_WhenQuoteDoesNotExist()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        await using var context = CreateContext(connection);

        var service = new QuoteToWorkOrderService(context);

        var workOrderId = await service.CreateOrGetWorkOrderFromQuoteAsync(999);

        Assert.Null(workOrderId);
    }
}