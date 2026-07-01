namespace WorkOrderFlow.Web.ViewModels;

public class StockAdjustmentViewModel
{
    public int InventoryItemId { get; set; }

    public string ItemName { get; set; } = string.Empty;

    public string? Sku { get; set; }

    public int CurrentQuantity { get; set; }

    public int QuantityChange { get; set; }

    public string? Notes { get; set; }

    public int NewQuantity => CurrentQuantity + QuantityChange;
}