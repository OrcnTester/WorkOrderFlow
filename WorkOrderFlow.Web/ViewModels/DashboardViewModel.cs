using WorkOrderFlow.Web.Models;

namespace WorkOrderFlow.Web.ViewModels;

public class DashboardViewModel
{
    public int TotalCustomers { get; set; }

    public int TotalQuotes { get; set; }

    public int PendingQuotes { get; set; }

    public int AcceptedQuotes { get; set; }

    public int OpenWorkOrders { get; set; }

    public int CompletedWorkOrders { get; set; }

    public int LateWorkOrders { get; set; }

    public decimal EstimatedRevenue { get; set; }

    public int TotalInventoryItems { get; set; }

    public int LowStockItemsCount { get; set; }

    public decimal InventoryCostValue { get; set; }

    public int MaterialsUsedCount { get; set; }

    public decimal MaterialUsageValue { get; set; }

    public int WorkOrdersWithMaterials { get; set; }

    public int TotalStockMovements { get; set; }

    public int ManualAdjustmentsCount { get; set; }

    public int WorkOrderStockMovementsCount { get; set; }

    public int WorkOrderNewCount { get; set; }

    public int WorkOrderApprovedCount { get; set; }

    public int WorkOrderInProgressCount { get; set; }

    public int WorkOrderWaitingPartsCount { get; set; }

    public int WorkOrderCompletedCount { get; set; }

    public int WorkOrderDeliveredCount { get; set; }

    public int WorkOrderCancelledCount { get; set; }

    public int QuoteDraftCount { get; set; }

    public int QuoteSentCount { get; set; }

    public int QuoteAcceptedCount { get; set; }

    public int QuoteRejectedCount { get; set; }

    public int QuoteExpiredCount { get; set; }

    public int InventoryHealthyItemsCount { get; set; }

    public List<WorkOrder> RecentWorkOrders { get; set; } = new();

    public List<InventoryItem> LowStockItems { get; set; } = new();

    public List<InventoryTransaction> RecentInventoryTransactions { get; set; } = new();
}