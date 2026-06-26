using System.ComponentModel.DataAnnotations.Schema;

namespace WorkOrderFlow.Web.Models;

public class InventoryItem
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Sku { get; set; }

    public string Category { get; set; } = "General";

    public int QuantityOnHand { get; set; }

    public int ReorderLevel { get; set; }

    public decimal UnitCost { get; set; }

    public decimal SalePrice { get; set; }

    public string? SupplierName { get; set; }

    public string? Location { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [NotMapped]
    public bool IsLowStock => QuantityOnHand <= ReorderLevel;
}