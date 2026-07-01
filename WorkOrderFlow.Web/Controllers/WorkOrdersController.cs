using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WorkOrderFlow.Web.Data;
using WorkOrderFlow.Web.Models;
using WorkOrderFlow.Web.Services;
namespace WorkOrderFlow.Web.Controllers;

public class WorkOrdersController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly WorkOrderPdfService _workOrderPdfService;
    private readonly WorkOrderWorkflowService _workflowService;

   public WorkOrdersController(
                                ApplicationDbContext context,
                                WorkOrderPdfService workOrderPdfService,
                                WorkOrderWorkflowService workflowService)
    {
        _context = context;
        _workOrderPdfService = workOrderPdfService;
        _workflowService = workflowService;
    }

   public async Task<IActionResult> Index(string? search, WorkOrderStatus? status, WorkOrderPriority? priority)
   {
        var workOrders = _context.WorkOrders
            .Include(w => w.Customer)
            .Include(w => w.Quote)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            workOrders = workOrders.Where(w =>
                w.Title.Contains(search) ||
                (w.Description != null && w.Description.Contains(search)) ||
                (w.Customer != null && w.Customer.FullName.Contains(search)));
        }

        if (status.HasValue)
        {
            workOrders = workOrders.Where(w => w.Status == status.Value);
        }

        if (priority.HasValue)
        {
            workOrders = workOrders.Where(w => w.Priority == priority.Value);
        }

        ViewData["CurrentSearch"] = search;
        ViewData["CurrentStatus"] = status.HasValue ? ((int)status.Value).ToString() : "";
        ViewData["CurrentPriority"] = priority.HasValue ? ((int)priority.Value).ToString() : "";

        return View(await workOrders
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
{
    if (id == null)
    {
        return NotFound();
    }

    var workOrder = await _context.WorkOrders
        .Include(w => w.Customer)
        .Include(w => w.Quote)
        .FirstOrDefaultAsync(m => m.Id == id);

    if (workOrder == null)
    {
        return NotFound();
    }

    var materials = await _context.WorkOrderMaterials
        .Include(m => m.InventoryItem)
        .Where(m => m.WorkOrderId == workOrder.Id)
        .OrderByDescending(m => m.UsedAt)
        .ToListAsync();

    var statusHistory = await _context.WorkOrderStatusHistories
        .Where(h => h.WorkOrderId == workOrder.Id)
        .OrderByDescending(h => h.CreatedAt)
        .ToListAsync();

    ViewData["Materials"] = materials;
    ViewData["MaterialTotal"] = materials.Sum(m => m.QuantityUsed * m.UnitPrice);
    ViewData["StatusHistory"] = statusHistory;

    return View(workOrder);
}

    public IActionResult Create()
    {
        ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "FullName");
        ViewData["QuoteId"] = new SelectList(_context.Quotes, "Id", "Title");

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("CustomerId,QuoteId,Title,Description,Status,Priority,DueDate,CompletedAt,ResolutionNote")] WorkOrder workOrder)
    {
        if (ModelState.IsValid)
        {
            workOrder.CreatedAt = DateTime.UtcNow;

            _context.Add(workOrder);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "FullName", workOrder.CustomerId);
        ViewData["QuoteId"] = new SelectList(_context.Quotes, "Id", "Title", workOrder.QuoteId);

        return View(workOrder);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var workOrder = await _context.WorkOrders.FindAsync(id);

        if (workOrder == null)
        {
            return NotFound();
        }

        ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "FullName", workOrder.CustomerId);
        ViewData["QuoteId"] = new SelectList(_context.Quotes, "Id", "Title", workOrder.QuoteId);

        return View(workOrder);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,CustomerId,QuoteId,Title,Description,Status,Priority,CreatedAt,DueDate,CompletedAt,ResolutionNote")] WorkOrder workOrder)
    {
        if (id != workOrder.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(workOrder);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!WorkOrderExists(workOrder.Id))
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "FullName", workOrder.CustomerId);
        ViewData["QuoteId"] = new SelectList(_context.Quotes, "Id", "Title", workOrder.QuoteId);

        return View(workOrder);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var workOrder = await _context.WorkOrders
            .Include(w => w.Customer)
            .Include(w => w.Quote)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (workOrder == null)
        {
            return NotFound();
        }

        return View(workOrder);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var workOrder = await _context.WorkOrders.FindAsync(id);

        if (workOrder != null)
        {
            _context.WorkOrders.Remove(workOrder);
        }

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> DownloadPdf(int id)
    {
        var workOrder = await _context.WorkOrders
            .Include(w => w.Customer)
            .Include(w => w.Quote)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (workOrder == null)
        {
            return NotFound();
        }

        var materials = await _context.WorkOrderMaterials
            .Include(m => m.InventoryItem)
            .Where(m => m.WorkOrderId == id)
            .OrderByDescending(m => m.UsedAt)
            .ToListAsync();

        var pdfBytes = _workOrderPdfService.Generate(workOrder, materials);

        Response.Headers.ContentDisposition = $"inline; filename=\"work-order-{workOrder.Id}.pdf\"";

        return File(pdfBytes, "application/pdf");
    }

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> StartWork(int id)
{
    var success = await _workflowService.ChangeStatusAsync(
        id,
        WorkOrderStatus.InProgress,
        "Work started or resumed",
        clearCompletedAt: true);

    if (!success)
    {
        return NotFound();
    }

    return RedirectToAction(nameof(Details), new { id });
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> MarkWaitingParts(int id)
{
    var success = await _workflowService.ChangeStatusAsync(
        id,
        WorkOrderStatus.WaitingParts,
        "Waiting for parts",
        clearCompletedAt: true);

    if (!success)
    {
        return NotFound();
    }

    return RedirectToAction(nameof(Details), new { id });
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> CompleteWork(int id)
{
    var success = await _workflowService.ChangeStatusAsync(
        id,
        WorkOrderStatus.Completed,
        "Work completed",
        setCompletedAt: true);

    if (!success)
    {
        return NotFound();
    }

    return RedirectToAction(nameof(Details), new { id });
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> MarkDelivered(int id)
{
    var success = await _workflowService.ChangeStatusAsync(
        id,
        WorkOrderStatus.Delivered,
        "Work order delivered",
        setCompletedAtIfMissing: true);

    if (!success)
    {
        return NotFound();
    }

    return RedirectToAction(nameof(Details), new { id });
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> ReopenWork(int id)
{
    var success = await _workflowService.ChangeStatusAsync(
        id,
        WorkOrderStatus.InProgress,
        "Work order reopened",
        clearCompletedAt: true);

    if (!success)
    {
        return NotFound();
    }

    return RedirectToAction(nameof(Details), new { id });
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> CancelWork(int id)
{
    var success = await _workflowService.ChangeStatusAsync(
        id,
        WorkOrderStatus.Cancelled,
        "Work order cancelled",
        clearCompletedAt: true);

    if (!success)
    {
        return NotFound();
    }

    return RedirectToAction(nameof(Details), new { id });
}

    private bool WorkOrderExists(int id)
    {
        return _context.WorkOrders.Any(e => e.Id == id);
    }
}