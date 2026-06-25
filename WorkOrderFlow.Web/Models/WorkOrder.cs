namespace WorkOrderFlow.Web.Models;

public class WorkOrder
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public Customer? Customer { get; set; }

    public int? QuoteId { get; set; }

    public Quote? Quote { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public WorkOrderStatus Status { get; set; } = WorkOrderStatus.New;

    public WorkOrderPriority Priority { get; set; } = WorkOrderPriority.Medium;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? DueDate { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string? ResolutionNote { get; set; }
}

public enum WorkOrderStatus
{
    New = 0,
    Approved = 1,
    InProgress = 2,
    WaitingParts = 3,
    Completed = 4,
    Delivered = 5,
    Cancelled = 6
}

public enum WorkOrderPriority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Urgent = 3
}