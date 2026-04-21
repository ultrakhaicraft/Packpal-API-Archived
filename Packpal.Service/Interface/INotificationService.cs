using Packpal.DAL.ModelViews.EntityModel;
using static Packpal.DAL.ModelViews.PayoutInfoModel;

namespace Packpal.BLL.Interface;

/// <summary>
/// Interface for notification service
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Notify keeper about new pending order
    /// </summary>
    /// <param name="keeperId">Keeper ID to notify</param>
    /// <param name="orderData">Order information</param>
    Task NotifyKeeperNewOrderAsync(Guid keeperId, object orderData);

    /// <summary>
    /// Notify keeper about order status change
    /// </summary>
    /// <param name="keeperId">Keeper ID to notify</param>
    /// <param name="orderId">Order ID</param>
    /// <param name="newStatus">New order status</param>
    Task NotifyKeeperOrderStatusChangeAsync(Guid keeperId, Guid orderId, string newStatus);

    /// <summary>
    /// Update keeper about current pending orders count
    /// </summary>
    /// <param name="keeperId">Keeper ID to notify</param>
    /// <param name="pendingCount">Current pending orders count</param>
    Task UpdateKeeperPendingCountAsync(Guid keeperId, int pendingCount);

    /// <summary>
    /// Notify renter about order status changes
    /// </summary>
    /// <param name="renterId">Renter ID to notify</param>
    /// <param name="orderId">Order ID</param>
    /// <param name="newStatus">New order status</param>
    /// <param name="keeperName">Name of the keeper</param>
    Task NotifyRenterOrderStatusChangeAsync(Guid renterId, Guid orderId, string newStatus, string keeperName);

    /// <summary>
    /// Notify Staff about Keeper create a payout request
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task NotifyStaffOfIncomingPayoutAsync(ViewPayoutInfo request);

    /// <summary>
    /// Notify keeper when their payout request has been completed
    /// </summary>
    /// <param name="keeperId">Keeper ID to notify</param>
    /// <param name="payoutAmount">Amount that was paid out</param>
    /// <param name="payoutId">Payout request ID</param>
    /// <returns></returns>
    Task NotifyKeeperPayoutCompletedAsync(Guid keeperId, decimal payoutAmount, Guid payoutId);

    /// <summary>
    /// Notify user about account ban
    /// </summary>
    /// <param name="userId">User ID to notify</param>
    /// <param name="reason">Reason for the ban</param>
    /// <param name="contactEmail">Contact email for support</param>
    /// <returns></returns>
    Task NotifyUserBannedAsync(Guid userId, string reason, string contactEmail);

    /// <summary>
    /// Notify staff about new keeper registration request
    /// </summary>
    /// <param name="userId">User ID who made the request</param>
    /// <param name="username">Username of the requester</param>
    /// <param name="requestId">Request ID</param>
    /// <returns></returns>
    Task NotifyStaffOfKeeperRegistrationAsync(Guid userId, string username, Guid requestId);

    /// <summary>
    /// Notify staff about new create storage request
    /// </summary>
    /// <param name="userId">User ID who made the request</param>
    /// <param name="username">Username of the requester</param>
    /// <param name="requestId">Request ID</param>
    /// <returns></returns>
    Task NotifyStaffOfCreateStorageAsync(Guid userId, string username, Guid requestId);

    /// <summary>
    /// Notify staff about new delete storage request
    /// </summary>
    /// <param name="userId">User ID who made the request</param>
    /// <param name="username">Username of the requester</param>
    /// <param name="requestId">Request ID</param>
    /// <returns></returns>
    Task NotifyStaffOfDeleteStorageAsync(Guid userId, string username, Guid requestId);

    /// <summary>
    /// Notify user that their keeper registration request has been approved
    /// </summary>
    /// <param name="userId">User ID to notify</param>
    /// <param name="message">Approval message</param>
    /// <returns></returns>
    Task NotifyUserKeeperRegistrationApprovedAsync(Guid userId, string message);

    /// <summary>
    /// Notify user that their keeper registration request has been rejected
    /// </summary>
    /// <param name="userId">User ID to notify</param>
    /// <param name="message">Rejection message with reason</param>
    /// <returns></returns>
    Task NotifyUserKeeperRegistrationRejectedAsync(Guid userId, string message);

    /// <summary>
    /// Send generic notification to a specific user via SignalR
    /// </summary>
    /// <param name="userId">User ID to send notification to</param>
    /// <param name="eventName">Event name for the notification</param>
    /// <param name="data">Notification data/payload</param>
    /// <returns></returns>
    Task SendToUserAsync(string userId, string eventName, object data);
}
