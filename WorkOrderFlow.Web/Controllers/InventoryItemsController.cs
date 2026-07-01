using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkOrderFlow.Web.Data;
using WorkOrderFlow.Web.Models;
using WorkOrderFlow.Web.ViewModels;

namespace WorkOrderFlow.Web.Controllers;

public class InventoryItemsController : Controller
{
    private readonly ApplicationDbContext _context;

    public InventoryItemsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? search, bool lowStockOnly = false)
    {
        var inventoryItems = _context.InventoryItems.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            inventoryItems = inventoryItems.Where(i =>
                i.Name.Contains(search) ||
                (i.Sku != null && i.Sku.Contains(search)) ||
                i.Category.Contains(search) ||
                (i.SupplierName != null && i.SupplierName.Contains(search)) ||
                (i.Location != null && i.Location.Contains(search)));
        }

        if (lowStockOnly)
        {
            inventoryItems = inventoryItems.Where(i => i.QuantityOnHand <= i.ReorderLevel);
        }

        ViewData["CurrentSearch"] = search;
        ViewData["LowStockOnly"] = lowStockOnly;

        return View(await inventoryItems
            .OrderBy(i => i.QuantityOnHand <= i.ReorderLevel ? 0 : 1)
            .ThenBy(i => i.Name)
            .ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var inventoryItem = await _context.InventoryItems
            .FirstOrDefaultAsync(m => m.Id == id);

        if (inventoryItem == null)
        {
            return NotFound();
        }

        return View(inventoryItem);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Sku,Category,QuantityOnHand,ReorderLevel,UnitCost,SalePrice,SupplierName,Location,Notes")] InventoryItem inventoryItem)
    {
        if (ModelState.IsValid)
        {
            inventoryItem.CreatedAt = DateTime.UtcNow;

            _context.InventoryItems.Add(inventoryItem);
            await _context.SaveChangesAsync();

            if (inventoryItem.QuantityOnHand != 0)
            {
                _context.InventoryTransactions.Add(new InventoryTransaction
                {
                    InventoryItemId = inventoryItem.Id,
                    WorkOrderId = null,
                    Type = InventoryTransactionType.ManualAdjustment,
                    QuantityChange = inventoryItem.QuantityOnHand,
                    QuantityAfter = inventoryItem.QuantityOnHand,
                    Notes = "Initial stock quantity"
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        return View(inventoryItem);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var inventoryItem = await _context.InventoryItems.FindAsync(id);

        if (inventoryItem == null)
        {
            return NotFound();
        }

        return View(inventoryItem);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Sku,Category,QuantityOnHand,ReorderLevel,UnitCost,SalePrice,SupplierName,Location,Notes")] InventoryItem formItem)
    {
        if (id != formItem.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(formItem);
        }

        var inventoryItem = await _context.InventoryItems.FindAsync(id);

        if (inventoryItem == null)
        {
            return NotFound();
        }

        var oldQuantity = inventoryItem.QuantityOnHand;

        inventoryItem.Name = formItem.Name;
        inventoryItem.Sku = formItem.Sku;
        inventoryItem.Category = formItem.Category;
        inventoryItem.QuantityOnHand = formItem.QuantityOnHand;
        inventoryItem.ReorderLevel = formItem.ReorderLevel;
        inventoryItem.UnitCost = formItem.UnitCost;
        inventoryItem.SalePrice = formItem.SalePrice;
        inventoryItem.SupplierName = formItem.SupplierName;
        inventoryItem.Location = formItem.Location;
        inventoryItem.Notes = formItem.Notes;

        var quantityChange = inventoryItem.QuantityOnHand - oldQuantity;

        if (quantityChange != 0)
        {
            _context.InventoryTransactions.Add(new InventoryTransaction
            {
                InventoryItemId = inventoryItem.Id,
                WorkOrderId = null,
                Type = InventoryTransactionType.ManualAdjustment,
                QuantityChange = quantityChange,
                QuantityAfter = inventoryItem.QuantityOnHand,
                Notes = "Manual adjustment from inventory edit"
            });
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!InventoryItemExists(inventoryItem.Id))
            {
                return NotFound();
            }

            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> AdjustStock(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var inventoryItem = await _context.InventoryItems.FindAsync(id);

        if (inventoryItem == null)
        {
            return NotFound();
        }

        var model = new StockAdjustmentViewModel
        {
            InventoryItemId = inventoryItem.Id,
            ItemName = inventoryItem.Name,
            Sku = inventoryItem.Sku,
            CurrentQuantity = inventoryItem.QuantityOnHand
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdjustStock(StockAdjustmentViewModel model)
    {
        var inventoryItem = await _context.InventoryItems.FindAsync(model.InventoryItemId);

        if (inventoryItem == null)
        {
            return NotFound();
        }

        model.ItemName = inventoryItem.Name;
        model.Sku = inventoryItem.Sku;
        model.CurrentQuantity = inventoryItem.QuantityOnHand;

        if (model.QuantityChange == 0)
        {
            ModelState.AddModelError("QuantityChange", "Quantity change cannot be zero.");
        }

        var newQuantity = inventoryItem.QuantityOnHand + model.QuantityChange;

        if (newQuantity < 0)
        {
            ModelState.AddModelError("QuantityChange", $"Stock cannot go below zero. Current quantity: {inventoryItem.QuantityOnHand}");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        inventoryItem.QuantityOnHand = newQuantity;

        _context.InventoryTransactions.Add(new InventoryTransaction
        {
            InventoryItemId = inventoryItem.Id,
            WorkOrderId = null,
            Type = InventoryTransactionType.ManualAdjustment,
            QuantityChange = model.QuantityChange,
            QuantityAfter = inventoryItem.QuantityOnHand,
            Notes = string.IsNullOrWhiteSpace(model.Notes)
                ? "Manual stock adjustment"
                : model.Notes
        });

        await _context.SaveChangesAsync();

        return RedirectToAction("Index", "InventoryTransactions");
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var inventoryItem = await _context.InventoryItems
            .FirstOrDefaultAsync(m => m.Id == id);

        if (inventoryItem == null)
        {
            return NotFound();
        }

        return View(inventoryItem);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var inventoryItem = await _context.InventoryItems.FindAsync(id);

        if (inventoryItem != null)
        {
            _context.InventoryItems.Remove(inventoryItem);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private bool InventoryItemExists(int id)
    {
        return _context.InventoryItems.Any(e => e.Id == id);
    }
}
