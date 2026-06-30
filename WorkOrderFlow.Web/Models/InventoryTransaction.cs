namespace WorkOrderFlow.Web.Models;

public class InventoryTransaction
{
    public int Id { get; set; }

    public int InventoryItemId { get; set; }

    public InventoryItem? InventoryItem { get; set; }

    public int? WorkOrderId { get; set; }

    public WorkOrder? WorkOrder { get; set; }

    public InventoryTransactionType Type { get; set; }

    public int QuantityChange { get; set; }

    public int QuantityAfter { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum InventoryTransactionType
{
    ManualAdjustment = 0,
    WorkOrderUsage = 1,
    WorkOrderUsageReversal = 2,
    WorkOrderUsageCorrection = 3
}