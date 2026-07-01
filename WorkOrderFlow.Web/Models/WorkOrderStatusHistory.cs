namespace WorkOrderFlow.Web.Models;

public class WorkOrderStatusHistory
{
    public int Id { get; set; }

    public int WorkOrderId { get; set; }

    public WorkOrder? WorkOrder { get; set; }

    public WorkOrderStatus? FromStatus { get; set; }

    public WorkOrderStatus ToStatus { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}