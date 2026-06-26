using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WorkOrderFlow.Web.Data;
using WorkOrderFlow.Web.Models;

namespace WorkOrderFlow.Web.Controllers;

public class WorkOrderMaterialsController : Controller
{
    private readonly ApplicationDbContext _context;

    public WorkOrderMaterialsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var materials = _context.WorkOrderMaterials
            .Include(w => w.WorkOrder)
            .Include(w => w.InventoryItem)
            .OrderByDescending(w => w.UsedAt);

        return View(await materials.ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var material = await _context.WorkOrderMaterials
            .Include(w => w.WorkOrder)
            .Include(w => w.InventoryItem)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (material == null)
        {
            return NotFound();
        }

        return View(material);
    }

    public IActionResult Create()
    {
        ViewData["WorkOrderId"] = new SelectList(_context.WorkOrders, "Id", "Title");
        ViewData["InventoryItemId"] = new SelectList(_context.InventoryItems, "Id", "Name");

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("WorkOrderId,InventoryItemId,QuantityUsed,UnitPrice,UsedAt,Notes")] WorkOrderMaterial workOrderMaterial)
    {
        var inventoryItem = await _context.InventoryItems.FindAsync(workOrderMaterial.InventoryItemId);

        if (inventoryItem == null)
        {
            ModelState.AddModelError("InventoryItemId", "Inventory item not found.");
        }
        else if (workOrderMaterial.QuantityUsed <= 0)
        {
            ModelState.AddModelError("QuantityUsed", "Quantity used must be greater than zero.");
        }
        else if (inventoryItem.QuantityOnHand < workOrderMaterial.QuantityUsed)
        {
            ModelState.AddModelError("QuantityUsed", $"Not enough stock. Available: {inventoryItem.QuantityOnHand}");
        }

        if (ModelState.IsValid)
        {
            if (workOrderMaterial.UnitPrice <= 0 && inventoryItem != null)
            {
                workOrderMaterial.UnitPrice = inventoryItem.SalePrice;
            }

            workOrderMaterial.UsedAt = workOrderMaterial.UsedAt == default
                ? DateTime.UtcNow
                : workOrderMaterial.UsedAt;

            inventoryItem!.QuantityOnHand -= workOrderMaterial.QuantityUsed;

            _context.Add(workOrderMaterial);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        ViewData["WorkOrderId"] = new SelectList(_context.WorkOrders, "Id", "Title", workOrderMaterial.WorkOrderId);
        ViewData["InventoryItemId"] = new SelectList(_context.InventoryItems, "Id", "Name", workOrderMaterial.InventoryItemId);

        return View(workOrderMaterial);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var material = await _context.WorkOrderMaterials.FindAsync(id);

        if (material == null)
        {
            return NotFound();
        }

        ViewData["WorkOrderId"] = new SelectList(_context.WorkOrders, "Id", "Title", material.WorkOrderId);
        ViewData["InventoryItemId"] = new SelectList(_context.InventoryItems, "Id", "Name", material.InventoryItemId);

        return View(material);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,WorkOrderId,InventoryItemId,QuantityUsed,UnitPrice,UsedAt,Notes")] WorkOrderMaterial workOrderMaterial)
    {
        if (id != workOrderMaterial.Id)
        {
            return NotFound();
        }

        var existingMaterial = await _context.WorkOrderMaterials
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (existingMaterial == null)
        {
            return NotFound();
        }

        var oldInventoryItem = await _context.InventoryItems.FindAsync(existingMaterial.InventoryItemId);
        var newInventoryItem = await _context.InventoryItems.FindAsync(workOrderMaterial.InventoryItemId);

        if (newInventoryItem == null)
        {
            ModelState.AddModelError("InventoryItemId", "Inventory item not found.");
        }
        else if (workOrderMaterial.QuantityUsed <= 0)
        {
            ModelState.AddModelError("QuantityUsed", "Quantity used must be greater than zero.");
        }

        if (ModelState.IsValid)
        {
            if (oldInventoryItem != null)
            {
                oldInventoryItem.QuantityOnHand += existingMaterial.QuantityUsed;
            }

            if (newInventoryItem!.QuantityOnHand < workOrderMaterial.QuantityUsed)
            {
                ModelState.AddModelError("QuantityUsed", $"Not enough stock. Available: {newInventoryItem.QuantityOnHand}");
            }
            else
            {
                newInventoryItem.QuantityOnHand -= workOrderMaterial.QuantityUsed;

                try
                {
                    _context.Update(workOrderMaterial);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WorkOrderMaterialExists(workOrderMaterial.Id))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }
        }

        ViewData["WorkOrderId"] = new SelectList(_context.WorkOrders, "Id", "Title", workOrderMaterial.WorkOrderId);
        ViewData["InventoryItemId"] = new SelectList(_context.InventoryItems, "Id", "Name", workOrderMaterial.InventoryItemId);

        return View(workOrderMaterial);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var material = await _context.WorkOrderMaterials
            .Include(w => w.WorkOrder)
            .Include(w => w.InventoryItem)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (material == null)
        {
            return NotFound();
        }

        return View(material);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var material = await _context.WorkOrderMaterials.FindAsync(id);

        if (material != null)
        {
            var inventoryItem = await _context.InventoryItems.FindAsync(material.InventoryItemId);

            if (inventoryItem != null)
            {
                inventoryItem.QuantityOnHand += material.QuantityUsed;
            }

            _context.WorkOrderMaterials.Remove(material);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private bool WorkOrderMaterialExists(int id)
    {
        return _context.WorkOrderMaterials.Any(e => e.Id == id);
    }
}