namespace Packpal.DAL.ModelViews.EntityModel;

public class OrderCountdownModel
{
    public Guid OrderId { get; set; }
    public DateTime StartKeepTime { get; set; }
    public int EstimatedDays { get; set; }
    public DateTime EstimatedEndTime { get; set; }
    public long TimeRemainingInMilliseconds { get; set; }
    public bool IsExpired { get; set; }
    public string FormattedTimeRemaining { get; set; } = string.Empty;
    public double PercentageComplete { get; set; }
}
