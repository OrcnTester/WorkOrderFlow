using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WorkOrderFlow.Web.Data;
using WorkOrderFlow.Web.Models;

namespace WorkOrderFlow.Web.Controllers
{
    public class WorkOrderMaterialsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WorkOrderMaterialsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: WorkOrderMaterials
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.WorkOrderMaterials.Include(w => w.InventoryItem).Include(w => w.WorkOrder);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: WorkOrderMaterials/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workOrderMaterial = await _context.WorkOrderMaterials
                .Include(w => w.InventoryItem)
                .Include(w => w.WorkOrder)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (workOrderMaterial == null)
            {
                return NotFound();
            }

            return View(workOrderMaterial);
        }

        // GET: WorkOrderMaterials/Create
        public IActionResult Create()
        {
            ViewData["InventoryItemId"] = new SelectList(_context.InventoryItems, "Id", "Id");
            ViewData["WorkOrderId"] = new SelectList(_context.WorkOrders, "Id", "Id");
            return View();
        }

        // POST: WorkOrderMaterials/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,WorkOrderId,InventoryItemId,QuantityUsed,UnitPrice,UsedAt,Notes")] WorkOrderMaterial workOrderMaterial)
        {
            if (ModelState.IsValid)
            {
                _context.Add(workOrderMaterial);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["InventoryItemId"] = new SelectList(_context.InventoryItems, "Id", "Id", workOrderMaterial.InventoryItemId);
            ViewData["WorkOrderId"] = new SelectList(_context.WorkOrders, "Id", "Id", workOrderMaterial.WorkOrderId);
            return View(workOrderMaterial);
        }

        // GET: WorkOrderMaterials/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workOrderMaterial = await _context.WorkOrderMaterials.FindAsync(id);
            if (workOrderMaterial == null)
            {
                return NotFound();
            }
            ViewData["InventoryItemId"] = new SelectList(_context.InventoryItems, "Id", "Id", workOrderMaterial.InventoryItemId);
            ViewData["WorkOrderId"] = new SelectList(_context.WorkOrders, "Id", "Id", workOrderMaterial.WorkOrderId);
            return View(workOrderMaterial);
        }

        // POST: WorkOrderMaterials/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,WorkOrderId,InventoryItemId,QuantityUsed,UnitPrice,UsedAt,Notes")] WorkOrderMaterial workOrderMaterial)
        {
            if (id != workOrderMaterial.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
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
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["InventoryItemId"] = new SelectList(_context.InventoryItems, "Id", "Id", workOrderMaterial.InventoryItemId);
            ViewData["WorkOrderId"] = new SelectList(_context.WorkOrders, "Id", "Id", workOrderMaterial.WorkOrderId);
            return View(workOrderMaterial);
        }

        // GET: WorkOrderMaterials/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workOrderMaterial = await _context.WorkOrderMaterials
                .Include(w => w.InventoryItem)
                .Include(w => w.WorkOrder)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (workOrderMaterial == null)
            {
                return NotFound();
            }

            return View(workOrderMaterial);
        }

        // POST: WorkOrderMaterials/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var workOrderMaterial = await _context.WorkOrderMaterials.FindAsync(id);
            if (workOrderMaterial != null)
            {
                _context.WorkOrderMaterials.Remove(workOrderMaterial);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool WorkOrderMaterialExists(int id)
        {
            return _context.WorkOrderMaterials.Any(e => e.Id == id);
        }
    }
}
