using Microsoft.AspNetCore.SignalR;

namespace Packpal.BLL.Hubs;

/// <summary>
/// SignalR Hub for real-time notifications to keepers
/// </summary>
//[Authorize] // Temporarily commented out for testing
public class KeeperNotificationHub : Hub
{

	/// <summary>
	/// Staff joins their specific notification group
	/// </summary>
	/// <param name="staffId">Staff ID</param>
	/// <returns></returns>
	public async Task JoinStaffGroup(string staffId)
	{
		await Groups.AddToGroupAsync(Context.ConnectionId, $"Staff_{staffId}");


		Console.WriteLine($"Staff {staffId} joined notification group. ConnectionId: {Context.ConnectionId}");
	}

	/// <summary>
	/// Staff joins their specific notification group
	/// </summary>
	/// <param name="staffId">Staff ID</param>
	/// <returns></returns>
	public async Task LeaveStaffGroup(string staffId)
	{
		await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Staff_{staffId}");


		Console.WriteLine($"Staff {staffId} left notification group. ConnectionId: {Context.ConnectionId}");
	}

	/// <summary>
	/// Joins a keeper to their specific notification group
	/// </summary>
	/// <param name="keeperId">The keeper's ID</param>
	public async Task JoinKeeperGroup(string keeperId)
    {
        var groupName = $"Keeper_{keeperId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        
        // Log the connection with more details
        Console.WriteLine($"[SignalR] Keeper {keeperId} joined notification group '{groupName}'. ConnectionId: {Context.ConnectionId}");
        Console.WriteLine($"[SignalR] Active connection count for group '{groupName}': [Group membership tracking not available]");
    }

	/// <summary>
	/// Leaves the keeper notification group
	/// </summary>
	/// <param name="keeperId">The keeper's ID</param>
	public async Task LeaveKeeperGroup(string keeperId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Keeper_{keeperId}");
        
        Console.WriteLine($"Keeper {keeperId} left notification group. ConnectionId: {Context.ConnectionId}");
    }

    /// <summary>
    /// Joins a renter to their specific notification group
    /// </summary>
    /// <param name="renterId">The renter's ID</param>
    public async Task JoinRenterGroup(string renterId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Renter_{renterId}");
        
        Console.WriteLine($"Renter {renterId} joined notification group. ConnectionId: {Context.ConnectionId}");
    }

    /// <summary>
    /// Leaves the renter notification group
    /// </summary>
    /// <param name="renterId">The renter's ID</param>
    public async Task LeaveRenterGroup(string renterId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Renter_{renterId}");
        
        Console.WriteLine($"Renter {renterId} left notification group. ConnectionId: {Context.ConnectionId}");
    }

    /// <summary>
    /// Called when a client connects
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        Console.WriteLine($"Client connected: {Context.ConnectionId}");
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
        if (exception != null)
        {
            Console.WriteLine($"Disconnect reason: {exception.Message}");
        }
        await base.OnDisconnectedAsync(exception);
    }
}
