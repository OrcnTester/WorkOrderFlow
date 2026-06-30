using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkOrderFlow.Web.Data;
using WorkOrderFlow.Web.Models;

namespace WorkOrderFlow.Web.Controllers;

public class InventoryTransactionsController : Controller
{
    private readonly ApplicationDbContext _context;

    public InventoryTransactionsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? search, InventoryTransactionType? type)
    {
        var transactions = _context.InventoryTransactions
            .Include(t => t.InventoryItem)
            .Include(t => t.WorkOrder)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            transactions = transactions.Where(t =>
                (t.InventoryItem != null &&
                    (t.InventoryItem.Name.Contains(search) ||
                     (t.InventoryItem.Sku != null && t.InventoryItem.Sku.Contains(search)))) ||
                (t.WorkOrder != null && t.WorkOrder.Title.Contains(search)) ||
                (t.Notes != null && t.Notes.Contains(search)));
        }

        if (type.HasValue)
        {
            transactions = transactions.Where(t => t.Type == type.Value);
        }

        ViewData["CurrentSearch"] = search;
        ViewData["CurrentType"] = type.HasValue ? ((int)type.Value).ToString() : "";

        return View(await transactions
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync());
    }
}