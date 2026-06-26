using System.ComponentModel.DataAnnotations.Schema;

namespace WorkOrderFlow.Web.Models;

public class WorkOrderMaterial
{
    public int Id { get; set; }

    public int WorkOrderId { get; set; }

    public WorkOrder? WorkOrder { get; set; }

    public int InventoryItemId { get; set; }

    public InventoryItem? InventoryItem { get; set; }

    public int QuantityUsed { get; set; }

    public decimal UnitPrice { get; set; }

    public DateTime UsedAt { get; set; } = DateTime.UtcNow;

    public string? Notes { get; set; }

    [NotMapped]
    public decimal LineTotal => QuantityUsed * UnitPrice;
}
