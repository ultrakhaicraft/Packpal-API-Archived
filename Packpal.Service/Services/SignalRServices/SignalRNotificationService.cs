using Microsoft.AspNetCore.SignalR;
using Packpal.BLL.Interface;
using Packpal.BLL.Hubs;
using static Packpal.DAL.ModelViews.PayoutInfoModel;

namespace Packpal.BLL.Services;

/// <summary>
/// SignalR implementation of notification service for real-time notifications
/// </summary>
public class SignalRNotificationService : INotificationService
{
    private readonly IHubContext<KeeperNotificationHub> _keeperHubContext;
    private readonly IHubContext<StaffNotificationHub> _staffHubContext;

    public SignalRNotificationService(
        IHubContext<KeeperNotificationHub> keeperHubContext,
        IHubContext<StaffNotificationHub> staffHubContext)
    {
        _keeperHubContext = keeperHubContext;
        _staffHubContext = staffHubContext;
    }

    /// <summary>
    /// Notify keeper about new pending order
    /// </summary>
    public async Task NotifyKeeperNewOrderAsync(Guid keeperId, object orderData)
    {
        try
        {
            var groupName = $"Keeper_{keeperId}";

            // Send notification to all connections in the keeper's group
            await _keeperHubContext.Clients.Group(groupName).SendAsync("NewOrderReceived", new
            {
                Type = "NEW_ORDER",
                Message = "You have a new order request!",
                Data = orderData,
                Timestamp = DateTime.UtcNow
            });

            Console.WriteLine($"[SignalR] Sent new order notification to keeper {keeperId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SignalR] Error sending new order notification: {ex.Message}");
            // Don't throw - notifications shouldn't break the main flow
        }
    }

    /// <summary>
    /// Notify keeper about order status change
    /// </summary>
    public async Task NotifyKeeperOrderStatusChangeAsync(Guid keeperId, Guid orderId, string newStatus)
    {
        try
        {
            var groupName = $"Keeper_{keeperId}";

            await _keeperHubContext.Clients.Group(groupName).SendAsync("OrderStatusChanged", new
            {
                Type = "ORDER_STATUS_CHANGE",
                Message = $"Order status changed to {newStatus}",
                Data = new
                {
                    OrderId = orderId,
                    NewStatus = newStatus,
                    CustomerName = "Customer", // TODO: Get actual customer name from order
                    Timestamp = DateTime.UtcNow
                },
                Timestamp = DateTime.UtcNow
            });

            Console.WriteLine($"[SignalR] Sent order status change notification to keeper {keeperId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SignalR] Error sending order status notification: {ex.Message}");
        }
    }

    /// <summary>
    /// Update keeper about current pending orders count
    /// </summary>
    public async Task UpdateKeeperPendingCountAsync(Guid keeperId, int pendingCount)
    {
        try
        {
            var groupName = $"Keeper_{keeperId}";

            await _keeperHubContext.Clients.Group(groupName).SendAsync("PendingCountUpdated", new
            {
                Type = "PENDING_COUNT_UPDATE",
                Data = new
                {
                    PendingOrdersCount = pendingCount
                },
                Timestamp = DateTime.UtcNow
            });

            Console.WriteLine($"[SignalR] Updated pending count for keeper {keeperId}: {pendingCount}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SignalR] Error updating pending count: {ex.Message}");
        }
    }

    /// <summary>
    /// Notify renter about order status change
    /// </summary>
    public async Task NotifyRenterOrderStatusChangeAsync(Guid renterId, Guid orderId, string newStatus, string keeperName)
    {
        try
        {
            var groupName = $"Renter_{renterId}";

            await _keeperHubContext.Clients.Group(groupName).SendAsync("OrderStatusChanged", new
            {
                Type = "ORDER_STATUS_CHANGE",
                Message = $"Your order status has been updated to {newStatus} by {keeperName}",
                Data = new
                {
                    OrderId = orderId,
                    NewStatus = newStatus,
                    KeeperName = keeperName,
                    Timestamp = DateTime.UtcNow
                },
                Timestamp = DateTime.UtcNow
            });

            Console.WriteLine($"[SignalR] Sent order status change notification to renter {renterId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SignalR] Error sending renter order status notification: {ex.Message}");
        }
    }

    /// <summary>
    /// Notify Staff when the Keeper create a payout request
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task NotifyStaffOfIncomingPayoutAsync(ViewPayoutInfo request)
    {
        // Optional: Create a DTO to send only necessary data
        var payload = new
        {
            request.Id,
            request.Amount,
            request.UserId,
            request.CreatedAt
        };

        // Send to all connected Staff clients
        await _staffHubContext.Clients.Group("Staff").SendAsync("ReceivePayoutRequest", payload);

        Console.WriteLine($"SignalR: Sent payout notification to staff. Payout ID: {request.Id}");
    }

    /// <summary>
    /// Notify user about account ban via SignalR
    /// </summary>
    public async Task NotifyUserBannedAsync(Guid userId, string reason, string contactEmail)
    {
        try
        {
            var groupName = $"User_{userId}";

            await _staffHubContext.Clients.Group(groupName).SendAsync("UserBanned", new
            {
                Type = "USER_BANNED",
                Message = "Your account has been suspended",
                Data = new
                {
                    Reason = reason,
                    ContactEmail = contactEmail,
                    Timestamp = DateTime.UtcNow
                },
                Timestamp = DateTime.UtcNow
            });

            Console.WriteLine($"[SignalR] Sent ban notification to user {userId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SignalR] Error sending ban notification: {ex.Message}");
        }
    }

    /// <summary>
    /// Notify staff about new keeper registration request
    /// </summary>
    public async Task NotifyStaffOfKeeperRegistrationAsync(Guid userId, string username, Guid requestId)
    {
        try
        {
            // Get all staff members - we'll need to notify all staff
            var notification = new
            {
                Type = "KEEPER_REGISTRATION_REQUEST",
                Message = $"New keeper registration request from {username}",
                Data = new
                {
                    UserId = userId,
                    Username = username,
                    RequestId = requestId,
                    RequestType = "KEEPER_REGISTRATION",
                    Timestamp = DateTime.UtcNow
                },
                Timestamp = DateTime.UtcNow
            };

            // Send to all staff members (we use a general Staff group)
            await _staffHubContext.Clients.Group("Staff").SendAsync("KeeperRegistrationRequest", notification);

            Console.WriteLine($"[SignalR] Sent keeper registration notification to staff for user {username} (ID: {userId})");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SignalR] Error sending keeper registration notification: {ex.Message}");
        }
    }

    /// <summary>
    /// Notify staff about new create storage request
    /// </summary>
    public async Task NotifyStaffOfCreateStorageAsync(Guid userId, string username, Guid requestId)
    {
        try
        {
            var notification = new
            {
                Type = "CREATE_STORAGE_REQUEST",
                Message = $"New storage creation request from {username}",
                Data = new
                {
                    UserId = userId,
                    Username = username,
                    RequestId = requestId,
                    RequestType = "CREATESTORAGE",
                    Timestamp = DateTime.UtcNow
                },
                Timestamp = DateTime.UtcNow
            };

            // Send to all staff members
            await _staffHubContext.Clients.Group("Staff").SendAsync("CreateStorageRequest", notification);

            Console.WriteLine($"[SignalR] Sent create storage notification to staff for user {username} (ID: {userId})");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SignalR] Error sending create storage notification: {ex.Message}");
        }
    }

    /// <summary>
    /// Notify staff about new delete storage request
    /// </summary>
    public async Task NotifyStaffOfDeleteStorageAsync(Guid userId, string username, Guid requestId)
    {
        try
        {
            var notification = new
            {
                Type = "DELETE_STORAGE_REQUEST",
                Message = $"New storage deletion request from {username}",
                Data = new
                {
                    UserId = userId,
                    Username = username,
                    RequestId = requestId,
                    RequestType = "DELETESTORAGE",
                    Timestamp = DateTime.UtcNow
                },
                Timestamp = DateTime.UtcNow
            };

            // Send to all staff members
            await _staffHubContext.Clients.Group("Staff").SendAsync("DeleteStorageRequest", notification);

            Console.WriteLine($"[SignalR] Sent delete storage notification to staff for user {username} (ID: {userId})");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SignalR] Error sending delete storage notification: {ex.Message}");
        }
    }

    /// <summary>
    /// Notify user that their keeper registration request has been approved
    /// </summary>
    public async Task NotifyUserKeeperRegistrationApprovedAsync(Guid userId, string message)
    {
        try
        {
            var notification = new
            {
                Type = "KEEPER_REGISTRATION_APPROVED",
                Message = message,
                Data = new
                {
                    UserId = userId,
                    Status = "APPROVED",
                    Timestamp = DateTime.UtcNow
                },
                Timestamp = DateTime.UtcNow
            };

            // Send to specific user via their group
            var groupName = $"User_{userId}";
            await _staffHubContext.Clients.Group(groupName).SendAsync("KeeperRegistrationResult", notification);

            Console.WriteLine($"[SignalR] Sent keeper registration approval notification to user {userId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SignalR] Error sending keeper registration approval notification: {ex.Message}");
        }
    }

    /// <summary>
    /// Notify user that their keeper registration request has been rejected
    /// </summary>
    public async Task NotifyUserKeeperRegistrationRejectedAsync(Guid userId, string message)
    {
        try
        {
            var notification = new
            {
                Type = "KEEPER_REGISTRATION_REJECTED",
                Message = message,
                Data = new
                {
                    UserId = userId,
                    Status = "REJECTED",
                    Timestamp = DateTime.UtcNow
                },
                Timestamp = DateTime.UtcNow
            };

            // Send to specific user via their group
            var groupName = $"User_{userId}";
            await _staffHubContext.Clients.Group(groupName).SendAsync("KeeperRegistrationResult", notification);

            Console.WriteLine($"[SignalR] Sent keeper registration rejection notification to user {userId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SignalR] Error sending keeper registration rejection notification: {ex.Message}");
        }
    }

    /// <summary>
    /// Send generic notification to a specific user via SignalR
    /// </summary>
    public async Task SendToUserAsync(string userId, string eventName, object data)
    {
        try
        {
            var groupName = $"User_{userId}";
            await _staffHubContext.Clients.Group(groupName).SendAsync(eventName, data);

            Console.WriteLine($"[SignalR] Sent {eventName} to user {userId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SignalR] Error sending {eventName} to user {userId}: {ex.Message}");
        }
    }

    /// <summary>
    /// Notify keeper when their payout request has been completed
    /// </summary>
    public async Task NotifyKeeperPayoutCompletedAsync(Guid keeperId, decimal payoutAmount, Guid payoutId)
    {
        try
        {
            var groupName = $"Keeper_{keeperId}";

            await _keeperHubContext.Clients.Group(groupName).SendAsync("PayoutCompleted", new
            {
                Type = "PAYOUT_COMPLETED",
                Message = $"Your payout of ${payoutAmount:F2} has been completed!",
                Data = new
                {
                    PayoutId = payoutId,
                    Amount = payoutAmount,
                    Status = "COMPLETED",
                    Timestamp = DateTime.UtcNow
                },
                Timestamp = DateTime.UtcNow
            });

            Console.WriteLine($"[SignalR] Sent payout completion notification to keeper {keeperId}. Amount: ${payoutAmount:F2}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SignalR] Error sending payout completion notification: {ex.Message}");
        }
    }
}
