using Microsoft.EntityFrameworkCore;
using WorkOrderFlow.Web.Data;
using WorkOrderFlow.Web.Models;

namespace WorkOrderFlow.Web.Services;

public class QuoteToWorkOrderService
{
    private readonly ApplicationDbContext _context;

    public QuoteToWorkOrderService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int?> CreateOrGetWorkOrderFromQuoteAsync(int quoteId)
    {
        var quote = await _context.Quotes
            .FirstOrDefaultAsync(q => q.Id == quoteId);

        if (quote == null)
        {
            return null;
        }

        var existingWorkOrder = await _context.WorkOrders
            .FirstOrDefaultAsync(w => w.QuoteId == quote.Id);

        if (existingWorkOrder != null)
        {
            if (quote.Status != QuoteStatus.Accepted)
            {
                quote.Status = QuoteStatus.Accepted;
                await _context.SaveChangesAsync();
            }

            return existingWorkOrder.Id;
        }

        quote.Status = QuoteStatus.Accepted;

        var workOrder = new WorkOrder
        {
            CustomerId = quote.CustomerId,
            QuoteId = quote.Id,
            Title = quote.Title,
            Description = quote.Notes,
            Status = WorkOrderStatus.New,
            Priority = WorkOrderPriority.Medium,
            CreatedAt = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(7)
        };

        _context.WorkOrders.Add(workOrder);
        await _context.SaveChangesAsync();

        return workOrder.Id;
    }
}