using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WorkOrderFlow.Web.Data;
using WorkOrderFlow.Web.Models;

namespace WorkOrderFlow.Web.Controllers;

public class WorkOrdersController : Controller
{
    private readonly ApplicationDbContext _context;

    public WorkOrdersController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var workOrders = _context.WorkOrders
            .Include(w => w.Customer)
            .Include(w => w.Quote)
            .OrderByDescending(w => w.CreatedAt);

        return View(await workOrders.ToListAsync());
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

    private bool WorkOrderExists(int id)
    {
        return _context.WorkOrders.Any(e => e.Id == id);
    }
}