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

        var estimatedRevenue = await _context.Quotes
            .Where(q => q.Status == QuoteStatus.Sent || q.Status == QuoteStatus.Accepted)
            .Select(q => (decimal?)(q.LaborCost + q.PartsCost - q.Discount))
            .SumAsync() ?? 0m;

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

            RecentWorkOrders = await _context.WorkOrders
                .Include(w => w.Customer)
                .Include(w => w.Quote)
                .OrderByDescending(w => w.CreatedAt)
                .Take(5)
                .ToListAsync()
        };

        return View(model);
    }
}