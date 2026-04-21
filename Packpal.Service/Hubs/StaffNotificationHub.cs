using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Packpal.BLL.Hubs
{
    public class StaffNotificationHub : Hub
    {
        private readonly ILogger<StaffNotificationHub> _logger;

        public StaffNotificationHub(ILogger<StaffNotificationHub> logger)
        {
            _logger = logger;
        }

        // Staff group methods
        public async Task JoinStaffGroup()
        {
            _logger.LogInformation($"🟡 JoinStaffGroup called for connection: {Context.ConnectionId}");
            
            try
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Staff");
                _logger.LogInformation($"✅ Staff {Context.ConnectionId} joined Staff group successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Failed to add {Context.ConnectionId} to Staff group: {ex.Message}");
            }
        }

        public async Task LeaveStaffGroup()
        {
            try
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Staff");
                _logger.LogInformation($"👋 Staff {Context.ConnectionId} left Staff group");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Failed to remove {Context.ConnectionId} from Staff group");
                throw;
            }
        }

        // User group methods
        public async Task JoinUserGroup(string userId)
        {
            _logger.LogInformation($"🟡 JoinUserGroup called for connection: {Context.ConnectionId}, User: {userId}");
            
            try
            {
                string groupName = $"User_{userId}";
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
                _logger.LogInformation($"✅ User {Context.ConnectionId} joined user group {groupName} successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Failed to add {Context.ConnectionId} to user group: {ex.Message}");
            }
        }

        public async Task LeaveUserGroup(string userId)
        {
            try
            {
                string groupName = $"User_{userId}";
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
                _logger.LogInformation($"👋 User {Context.ConnectionId} left user group {groupName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Failed to remove {Context.ConnectionId} from user group");
                throw;
            }
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation($"🔗 Client connected: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            try
            {
                // Client will be automatically removed from all groups
                _logger.LogInformation($"🔌 Client disconnected: {Context.ConnectionId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error during disconnection for {Context.ConnectionId}");
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
