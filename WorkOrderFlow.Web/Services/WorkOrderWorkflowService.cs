using Microsoft.EntityFrameworkCore;
using WorkOrderFlow.Web.Data;
using WorkOrderFlow.Web.Models;

namespace WorkOrderFlow.Web.Services;

public class WorkOrderWorkflowService
{
    private readonly ApplicationDbContext _context;

    public WorkOrderWorkflowService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ChangeStatusAsync(
        int workOrderId,
        WorkOrderStatus toStatus,
        string notes,
        bool clearCompletedAt = false,
        bool setCompletedAt = false,
        bool setCompletedAtIfMissing = false)
    {
        var workOrder = await _context.WorkOrders
            .FirstOrDefaultAsync(w => w.Id == workOrderId);

        if (workOrder == null)
        {
            return false;
        }

        var fromStatus = workOrder.Status;

        if (fromStatus != toStatus)
        {
            _context.WorkOrderStatusHistories.Add(new WorkOrderStatusHistory
            {
                WorkOrderId = workOrder.Id,
                FromStatus = fromStatus,
                ToStatus = toStatus,
                Notes = notes,
                CreatedAt = DateTime.UtcNow
            });

            workOrder.Status = toStatus;
        }

        if (clearCompletedAt)
        {
            workOrder.CompletedAt = null;
        }

        if (setCompletedAt)
        {
            workOrder.CompletedAt = DateTime.UtcNow;
        }

        if (setCompletedAtIfMissing && workOrder.CompletedAt == null)
        {
            workOrder.CompletedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return true;
    }
}