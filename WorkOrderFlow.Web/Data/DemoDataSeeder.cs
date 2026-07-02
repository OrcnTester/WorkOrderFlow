using Microsoft.EntityFrameworkCore;
using WorkOrderFlow.Web.Models;

namespace WorkOrderFlow.Web.Data;

public static class DemoDataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Customers.AnyAsync())
        {
            return;
        }

        var customer = new Customer
        {
            FullName = "Ahmet Yılmaz",
            CompanyName = "Yılmaz Servis",
            Phone = "0555 000 00 00",
            Email = "ahmet.yilmaz@example.com",
            Address = "Eskişehir",
            Notes = "Demo customer for WorkOrderFlow.",
            CreatedAt = DateTime.UtcNow.AddDays(-7)
        };

        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        var quote = new Quote
        {
            CustomerId = customer.Id,
            Title = "RKS R250 Zincir bakım teklifi",
            LaborCost = 750,
            PartsCost = 350,
            Discount = 100,
            Status = QuoteStatus.Accepted,
            ValidUntil = DateTime.UtcNow.AddDays(15),
            Notes = "Zincir temizliği, yağlama ve genel kontrol.",
            CreatedAt = DateTime.UtcNow.AddDays(-6)
        };

        context.Quotes.Add(quote);
        await context.SaveChangesAsync();

        var chainOil = new InventoryItem
        {
            Name = "Zincir Yağı",
            Sku = "CHAIN-OIL-001",
            Category = "Maintenance",
            QuantityOnHand = 10,
            ReorderLevel = 3,
            UnitCost = 180,
            SalePrice = 250,
            SupplierName = "Demo Supplier",
            Location = "Shelf A1",
            Notes = "Demo inventory item.",
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        };

        var brakePad = new InventoryItem
        {
            Name = "Fren Balatası",
            Sku = "BRAKE-PAD-001",
            Category = "Brake",
            QuantityOnHand = 2,
            ReorderLevel = 3,
            UnitCost = 220,
            SalePrice = 350,
            SupplierName = "Demo Supplier",
            Location = "Shelf B2",
            Notes = "Low stock demo item.",
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        };

        context.InventoryItems.AddRange(chainOil, brakePad);
        await context.SaveChangesAsync();

        var workOrder = new WorkOrder
        {
            CustomerId = customer.Id,
            QuoteId = quote.Id,
            Title = "RKS R250 Zincir bakım işi",
            Description = "Zincir temizliği, yağlama ve genel kontrol yapılacak.",
            Status = WorkOrderStatus.InProgress,
            Priority = WorkOrderPriority.Medium,
            CreatedAt = DateTime.UtcNow.AddDays(-4),
            DueDate = DateTime.UtcNow.AddDays(3)
        };

        context.WorkOrders.Add(workOrder);
        await context.SaveChangesAsync();

        context.WorkOrderStatusHistories.AddRange(
            new WorkOrderStatusHistory
            {
                WorkOrderId = workOrder.Id,
                FromStatus = WorkOrderStatus.New,
                ToStatus = WorkOrderStatus.InProgress,
                Notes = "Work started",
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            }
        );

        var material = new WorkOrderMaterial
        {
            WorkOrderId = workOrder.Id,
            InventoryItemId = chainOil.Id,
            QuantityUsed = 1,
            UnitPrice = 250,
            UsedAt = DateTime.UtcNow.AddDays(-2),
            Notes = "Used for chain maintenance."
        };

        context.WorkOrderMaterials.Add(material);

        chainOil.QuantityOnHand -= 1;

        context.InventoryTransactions.Add(new InventoryTransaction
        {
            InventoryItemId = chainOil.Id,
            WorkOrderId = workOrder.Id,
            Type = InventoryTransactionType.WorkOrderUsage,
            QuantityChange = -1,
            QuantityAfter = chainOil.QuantityOnHand,
            Notes = $"Material used on work order #{workOrder.Id}",
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        });

        context.InventoryTransactions.Add(new InventoryTransaction
        {
            InventoryItemId = brakePad.Id,
            Type = InventoryTransactionType.ManualAdjustment,
            QuantityChange = 2,
            QuantityAfter = brakePad.QuantityOnHand,
            Notes = "Initial demo stock count",
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        });

        await context.SaveChangesAsync();
    }
}