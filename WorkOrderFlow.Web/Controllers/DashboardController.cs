using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkOrderFlow.Web.Data;
using WorkOrderFlow.Web.Models;
using WorkOrderFlow.Web.ViewModels;

namespace WorkOrderFlow.Web.Controllers;

public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var now = DateTime.UtcNow;

        var inventoryItems = await _context.InventoryItems
            .OrderBy(i => i.Name)
            .ToListAsync();

        var materialUsages = await _context.WorkOrderMaterials
            .Include(m => m.InventoryItem)
            .ToListAsync();

        var estimatedRevenue = await _context.Quotes
            .Where(q => q.Status == QuoteStatus.Sent || q.Status == QuoteStatus.Accepted)
            .Select(q => (decimal?)(q.LaborCost + q.PartsCost - q.Discount))
            .SumAsync() ?? 0m;

        var lowStockItems = inventoryItems
            .Where(i => i.QuantityOnHand <= i.ReorderLevel)
            .OrderBy(i => i.QuantityOnHand)
            .Take(5)
            .ToList();

        var model = new DashboardViewModel
        {
            TotalCustomers = await _context.Customers.CountAsync(),

            TotalQuotes = await _context.Quotes.CountAsync(),

            PendingQuotes = await _context.Quotes
                .CountAsync(q => q.Status == QuoteStatus.Draft || q.Status == QuoteStatus.Sent),

            AcceptedQuotes = await _context.Quotes
                .CountAsync(q => q.Status == QuoteStatus.Accepted),

            OpenWorkOrders = await _context.WorkOrders
                .CountAsync(w =>
                    w.Status != WorkOrderStatus.Completed &&
                    w.Status != WorkOrderStatus.Delivered &&
                    w.Status != WorkOrderStatus.Cancelled),

            CompletedWorkOrders = await _context.WorkOrders
                .CountAsync(w =>
                    w.Status == WorkOrderStatus.Completed ||
                    w.Status == WorkOrderStatus.Delivered),

            LateWorkOrders = await _context.WorkOrders
                .CountAsync(w =>
                    w.DueDate != null &&
                    w.DueDate < now &&
                    w.Status != WorkOrderStatus.Completed &&
                    w.Status != WorkOrderStatus.Delivered &&
                    w.Status != WorkOrderStatus.Cancelled),

            EstimatedRevenue = estimatedRevenue,

            TotalInventoryItems = inventoryItems.Count,

            LowStockItemsCount = lowStockItems.Count,

            InventoryCostValue = inventoryItems.Sum(i => i.QuantityOnHand * i.UnitCost),

            MaterialsUsedCount = materialUsages.Count,

            MaterialUsageValue = materialUsages.Sum(m => m.QuantityUsed * m.UnitPrice),

            WorkOrdersWithMaterials = materialUsages
                .Select(m => m.WorkOrderId)
                .Distinct()
                .Count(),

            TotalStockMovements = await _context.InventoryTransactions.CountAsync(),

            ManualAdjustmentsCount = await _context.InventoryTransactions
                .CountAsync(t => t.Type == InventoryTransactionType.ManualAdjustment),

            WorkOrderStockMovementsCount = await _context.InventoryTransactions
                .CountAsync(t =>
                    t.Type == InventoryTransactionType.WorkOrderUsage ||
                    t.Type == InventoryTransactionType.WorkOrderUsageReversal ||
                    t.Type == InventoryTransactionType.WorkOrderUsageCorrection),

            WorkOrderNewCount = await _context.WorkOrders.CountAsync(w => w.Status == WorkOrderStatus.New),
            WorkOrderApprovedCount = await _context.WorkOrders.CountAsync(w => w.Status == WorkOrderStatus.Approved),
            WorkOrderInProgressCount = await _context.WorkOrders.CountAsync(w => w.Status == WorkOrderStatus.InProgress),
            WorkOrderWaitingPartsCount = await _context.WorkOrders.CountAsync(w => w.Status == WorkOrderStatus.WaitingParts),
            WorkOrderCompletedCount = await _context.WorkOrders.CountAsync(w => w.Status == WorkOrderStatus.Completed),
            WorkOrderDeliveredCount = await _context.WorkOrders.CountAsync(w => w.Status == WorkOrderStatus.Delivered),
            WorkOrderCancelledCount = await _context.WorkOrders.CountAsync(w => w.Status == WorkOrderStatus.Cancelled),

            QuoteDraftCount = await _context.Quotes.CountAsync(q => q.Status == QuoteStatus.Draft),
            QuoteSentCount = await _context.Quotes.CountAsync(q => q.Status == QuoteStatus.Sent),
            QuoteAcceptedCount = await _context.Quotes.CountAsync(q => q.Status == QuoteStatus.Accepted),
            QuoteRejectedCount = await _context.Quotes.CountAsync(q => q.Status == QuoteStatus.Rejected),
            QuoteExpiredCount = await _context.Quotes.CountAsync(q => q.Status == QuoteStatus.Expired),

            InventoryHealthyItemsCount = inventoryItems.Count - lowStockItems.Count,

            RecentWorkOrders = await _context.WorkOrders
                .Include(w => w.Customer)
                .Include(w => w.Quote)
                .OrderByDescending(w => w.CreatedAt)
                .Take(5)
                .ToListAsync(),

            LowStockItems = lowStockItems,

            RecentInventoryTransactions = await _context.InventoryTransactions
                .Include(t => t.InventoryItem)
                .Include(t => t.WorkOrder)
                .OrderByDescending(t => t.CreatedAt)
                .Take(5)
                .ToListAsync()
        };

        return View(model);
    }
}