using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WorkOrderFlow.Web.Data;
using WorkOrderFlow.Web.Services;
using WorkOrderFlow.Web.Models;

namespace WorkOrderFlow.Web.Controllers
{
    public class QuotesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly QuotePdfService _quotePdfService;
        private readonly QuoteToWorkOrderService _quoteToWorkOrderService;
        public QuotesController(
                                ApplicationDbContext context,
                                QuotePdfService quotePdfService,
                                QuoteToWorkOrderService quoteToWorkOrderService)
        {
            _context = context;
            _quotePdfService = quotePdfService;
            _quoteToWorkOrderService = quoteToWorkOrderService;
        }

        // GET: Quotes
      public async Task<IActionResult> Index(string? search, QuoteStatus? status)
      {
            var quotes = _context.Quotes
                .Include(q => q.Customer)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                quotes = quotes.Where(q =>
                    q.Title.Contains(search) ||
                    (q.Notes != null && q.Notes.Contains(search)) ||
                    (q.Customer != null && q.Customer.FullName.Contains(search)));
            }

            if (status.HasValue)
            {
                quotes = quotes.Where(q => q.Status == status.Value);
            }

            ViewData["CurrentSearch"] = search;
            ViewData["CurrentStatus"] = status.HasValue ? ((int)status.Value).ToString() : "";

            return View(await quotes
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync());
      }

        // GET: Quotes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quote = await _context.Quotes
                .Include(q => q.Customer)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (quote == null)
            {
                return NotFound();
            }

            return View(quote);
        }

        // GET: Quotes/Create
        public IActionResult Create()
        {
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Id");
            return View();
        }

        // POST: Quotes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CustomerId,Title,LaborCost,PartsCost,Discount,Status,CreatedAt,ValidUntil,Notes")] Quote quote)
        {
            if (ModelState.IsValid)
            {
                _context.Add(quote);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Id", quote.CustomerId);
            return View(quote);
        }

        // GET: Quotes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quote = await _context.Quotes.FindAsync(id);
            if (quote == null)
            {
                return NotFound();
            }
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Id", quote.CustomerId);
            return View(quote);
        }

        // POST: Quotes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CustomerId,Title,LaborCost,PartsCost,Discount,Status,CreatedAt,ValidUntil,Notes")] Quote quote)
        {
            if (id != quote.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(quote);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!QuoteExists(quote.Id))
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
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Id", quote.CustomerId);
            return View(quote);
        }

        // GET: Quotes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quote = await _context.Quotes
                .Include(q => q.Customer)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (quote == null)
            {
                return NotFound();
            }

            return View(quote);
        }

        // POST: Quotes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var quote = await _context.Quotes.FindAsync(id);
            if (quote != null)
            {
                _context.Quotes.Remove(quote);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DownloadPdf(int id)
        {
            var quote = await _context.Quotes
                .Include(q => q.Customer)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quote == null)
            {
                return NotFound();
            }

            var pdfBytes = _quotePdfService.Generate(quote);

            Response.Headers.ContentDisposition = $"inline; filename=\"quote-{quote.Id}.pdf\"";

            return File(pdfBytes, "application/pdf");
        }

public async Task<IActionResult> CreateWorkOrderFromQuote(int id)
{
    var workOrderId = await _quoteToWorkOrderService.CreateOrGetWorkOrderFromQuoteAsync(id);

    if (workOrderId == null)
    {
        return NotFound();
    }

    return RedirectToAction("Details", "WorkOrders", new { id = workOrderId.Value });
}
        private bool QuoteExists(int id)
        {
            return _context.Quotes.Any(e => e.Id == id);
        }
    }
}
