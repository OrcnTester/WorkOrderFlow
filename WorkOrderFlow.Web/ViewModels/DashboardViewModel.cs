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

    public List<WorkOrder> RecentWorkOrders { get; set; } = new();
}