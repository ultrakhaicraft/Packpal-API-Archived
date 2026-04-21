using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Packpal.BLL.Interface;
using Packpal.Contract.Repositories.Interface;
using Packpal.DAL.Entity;
using Packpal.DAL.Enum;

namespace Packpal.Controllers;

[ApiController]
[Route("payment")]
public class PaymentReturnController : ControllerBase
{
    private readonly ILogger<PaymentReturnController> _logger;
    private readonly IOrderService _orderService;
    private readonly ITransactionService _transactionService;
    private readonly INotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;

    public PaymentReturnController(ILogger<PaymentReturnController> logger, IOrderService orderService, ITransactionService transactionService, INotificationService notificationService, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _orderService = orderService;
        _transactionService = transactionService;
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handle PayOS payment success callback
    /// </summary>
    [HttpGet("success")]
    public async Task<IActionResult> PaymentSuccess([FromQuery] string orderId, [FromQuery] string code, [FromQuery] string id, [FromQuery] bool cancel, [FromQuery] string status, [FromQuery] long orderCode)
    {
        _logger.LogInformation("🔥 PaymentReturnController.PaymentSuccess CALLED - OrderId: {OrderId}, Code: {Code}, Status: {Status}, OrderCode: {OrderCode}", orderId, code, status, orderCode);

        try
        {
            // Update order payment status if orderId is provided
            if (!string.IsNullOrEmpty(orderId) && Guid.TryParse(orderId, out var orderGuid))
            {
                _logger.LogInformation("🔥 Updating payment status for order: {OrderId}", orderGuid);
                var updateResult = await _orderService.UpdateIsPaidAsync(orderGuid);
                
                if (updateResult)
                {
                    _logger.LogInformation("✅ Order payment status updated successfully: {OrderId}", orderGuid);
                    
                    // Find and update related IN transaction status
                    try
                    {
                        _logger.LogInformation("🔥 Looking for transactions with OrderId: {OrderId}", orderGuid);
                        
                        var transactions = await _unitOfWork.GetRepository<Transaction>()
                            .Entities
                            .Where(t => t.OrderId == orderGuid)
                            .ToListAsync();
                            
                        _logger.LogInformation("🔥 Found {Count} transactions for order", transactions.Count);
                        
                        var inTransaction = transactions.FirstOrDefault(t => t.TransactionType == TransactionTypeEnum.IN.ToString());
                        
                        if (inTransaction != null)
                        {
                            _logger.LogInformation("🔥 Found IN transaction: {TransactionId}, current status: {Status}", inTransaction.Id, inTransaction.Status);
                            
                            await _transactionService.ChangeStatus(inTransaction.Id.ToString(), TransactionStatus.COMPLETED.ToString());
                            _logger.LogInformation("✅ Transaction status updated to COMPLETED: {TransactionId}", inTransaction.Id);
                            
                            // Get order details for notifications
                            var orderDetails = await _orderService.GetOrderDetailByIdAsync(orderGuid);
                            if (orderDetails != null)
                            {
                                try
                                {
                                    // Get storage information to find keeper ID
                                    var storage = await _unitOfWork.GetRepository<Storage>()
                                        .Entities
                                        .Include(s => s.Keeper)
                                            .ThenInclude(k => k!.User)
                                        .FirstOrDefaultAsync(s => s.Id == orderDetails.StorageId);

                                    if (storage?.Keeper != null)
                                    {
                                        // Notify keeper about successful payment
                                        await _notificationService.NotifyKeeperOrderStatusChangeAsync(
                                            storage.KeeperId,
                                            orderGuid,
                                            "PAID"
                                        );

                                        // Notify renter about payment confirmation
                                        await _notificationService.NotifyRenterOrderStatusChangeAsync(
                                            orderDetails.RenterId,
                                            orderGuid,
                                            "PAID",
                                            storage.Keeper.User?.Username ?? "Storage Keeper"
                                        );
                                        
                                        _logger.LogInformation("✅ Payment success notifications sent for order: {OrderId}", orderGuid);
                                    }
                                }
                                catch (Exception notifyEx)
                                {
                                    _logger.LogError(notifyEx, "Failed to send payment success notifications for order: {OrderId}", orderGuid);
                                    // Don't fail the payment update if notification fails
                                }
                            }
                        }
                        else
                        {
                            _logger.LogWarning("🔥 No IN transaction found for order: {OrderId}. Available transactions: {TransactionTypes}", 
                                orderGuid, string.Join(", ", transactions.Select(t => t.TransactionType)));
                        }
                    }
                    catch (Exception transEx)
                    {
                        _logger.LogError(transEx, "Failed to update transaction status for order: {OrderId}", orderGuid);
                        // Don't fail the payment update if transaction update fails
                    }
                }
                else
                {
                    _logger.LogWarning("⚠️ Failed to update order payment status or order already paid: {OrderId}", orderGuid);
                }
            }
            else
            {
                _logger.LogWarning("⚠️ Invalid or missing orderId in payment success callback: {OrderId}", orderId);
            }

            // Create URLs for deeplinks
            var deepLinkUrl = $"packpals://payment/success?orderId={orderId}&status=success&orderCode={orderCode}";
            var androidIntentUrl = $"intent://payment/success?orderId={Uri.EscapeDataString(orderId ?? "")}&status=success&orderCode={orderCode}#Intent;scheme=packpals;package=com.anonymous.PackPals;end";
            
            var html = $@"<!DOCTYPE html>
<html>
<head>
    <title>Payment Successful - PackPals</title>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1'>
    <style>
        body {{ 
            font-family: Arial, sans-serif; 
            text-align: center; 
            background: linear-gradient(135deg, #4CAF50 0%, #45a049 100%);
            color: white;
            margin: 0;
            padding: 20px;
            min-height: 100vh;
            display: flex;
            flex-direction: column;
            justify-content: center;
        }}
        .container {{
            background: rgba(255,255,255,0.1);
            border-radius: 20px;
            padding: 40px;
            max-width: 500px;
            margin: 0 auto;
            backdrop-filter: blur(10px);
            box-shadow: 0 8px 32px rgba(0,0,0,0.3);
        }}
        .success-icon {{
            font-size: 80px;
            margin-bottom: 20px;
            animation: bounce 1s infinite alternate;
        }}
        @keyframes bounce {{
            from {{ transform: translateY(0px); }}
            to {{ transform: translateY(-10px); }}
        }}
        .btn {{
            background: rgba(255,255,255,0.9);
            border: none;
            color: #4CAF50;
            padding: 15px 30px;
            border-radius: 30px;
            text-decoration: none;
            display: inline-block;
            margin: 15px;
            font-weight: bold;
            font-size: 16px;
            transition: all 0.3s ease;
            cursor: pointer;
        }}
        .btn:hover {{
            background: white;
            transform: translateY(-2px);
            box-shadow: 0 4px 15px rgba(0,0,0,0.2);
        }}
        .debug {{
            background: rgba(0,0,0,0.2);
            padding: 15px;
            border-radius: 10px;
            font-size: 12px;
            margin-top: 20px;
            font-family: monospace;
            text-align: left;
            max-height: 200px;
            overflow-y: auto;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='success-icon'>✅</div>
        <h1>Payment Successful!</h1>
        <p style='font-size: 18px; margin: 20px 0;'>Your payment has been processed successfully.</p>
        <p><strong>Order ID:</strong> {orderId ?? "N/A"}</p>
        <p><strong>Order Code:</strong> {orderCode}</p>
        
        <div style='margin-top: 30px;'>
            <button class='btn' onclick='tryOpenApp()'>
                📱 Return to PackPals App
            </button>
            <br>
            <button class='btn' onclick='tryAndroidIntent()' style='background: rgba(255,255,255,0.7);'>
                🤖 Try Android Intent
            </button>
        </div>
        
        <div class='debug' id='debugLog'>
            <strong>🔍 Debug Log:</strong><br>
            Page loaded...<br>
        </div>
        
        <p style='font-size: 12px; opacity: 0.8; margin-top: 20px;'>
            If the app doesn't open, please ensure PackPals is installed and try again.
        </p>
    </div>

    <script>
        var deepLinkUrl = '{deepLinkUrl}';
        var androidIntentUrl = '{androidIntentUrl}';

        function addLog(message) {{
            var log = document.getElementById('debugLog');
            var time = new Date().toLocaleTimeString();
            log.innerHTML += '[' + time + '] ' + message + '<br>';
            log.scrollTop = log.scrollHeight;
            console.log('[PackPals] ' + message);
        }}

        function tryOpenApp() {{
            addLog('🚀 User clicked! Attempting to open app with: ' + deepLinkUrl);
            
            try {{
                window.location.href = deepLinkUrl;
                addLog('✅ Tried window.location.href');
                
                setTimeout(function() {{
                    addLog('⏰ If app did not open, try Android Intent button');
                }}, 2000);
                
            }} catch (e) {{
                addLog('❌ window.location.href failed: ' + e.message);
                
                try {{
                    var link = document.createElement('a');
                    link.href = deepLinkUrl;
                    link.style.display = 'none';
                    document.body.appendChild(link);
                    link.click();
                    document.body.removeChild(link);
                    addLog('✅ Fallback: Tried programmatic click');
                }} catch (e2) {{
                    addLog('❌ All methods failed. Try Android Intent button.');
                }}
            }}
        }}
        
        function tryAndroidIntent() {{
            addLog('🤖 User clicked Android Intent! Trying: ' + androidIntentUrl);
            
            try {{
                window.location.href = androidIntentUrl;
                addLog('✅ Android intent launched');
                
                setTimeout(function() {{
                    addLog('📱 If still not working, check if PackPals app is installed');
                }}, 3000);
                
            }} catch (e) {{
                addLog('❌ Android intent failed: ' + e.message);
                addLog('💡 Make sure PackPals app is installed from APK/Play Store');
            }}
        }}
        
        document.addEventListener('DOMContentLoaded', function() {{
            addLog('📄 Page fully loaded');
            addLog('📱 User Agent: ' + navigator.userAgent);
            addLog('⚠️ Click Return to PackPals App button to open app');
            
            var isMobile = /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent);
            if (isMobile) {{
                addLog('📱 Mobile device detected');
            }} else {{
                addLog('💻 Desktop browser - deeplinks may not work');
            }}
        }});
        
        var startTime = Date.now();
        
        window.addEventListener('blur', function() {{
            addLog('👁️ Page lost focus - app might have opened!');
        }});
        
        window.addEventListener('focus', function() {{
            var timeAway = Date.now() - startTime;
            if (timeAway > 3000) {{
                addLog('👁️ Page regained focus after ' + timeAway + 'ms - returned from app?');
            }}
            startTime = Date.now();
        }});
        
        window.addEventListener('beforeunload', function() {{
            addLog('🚪 Page unloading - app opened successfully!');
        }});
    </script>
</body>
</html>";

            return Content(html, "text/html");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling payment success");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Handle PayOS payment cancel callback
    /// </summary>
    [HttpGet("cancel")]
    public IActionResult PaymentCancel([FromQuery] string orderId, [FromQuery] string code, [FromQuery] string id, [FromQuery] bool cancel, [FromQuery] string status, [FromQuery] long orderCode)
    {
        _logger.LogInformation("Payment Cancelled - OrderId: {OrderId}, Code: {Code}, Status: {Status}", orderId, code, status);

        try
        {
            // Create URLs for deeplinks
            var deepLinkUrl = $"packpals://payment/cancel?orderId={orderId}&status=cancelled&orderCode={orderCode}";
            var androidIntentUrl = $"intent://payment/cancel?orderId={Uri.EscapeDataString(orderId ?? "")}&status=cancelled&orderCode={orderCode}#Intent;scheme=packpals;package=com.anonymous.PackPals;end";
            
            var html = $@"<!DOCTYPE html>
<html>
<head>
    <title>Payment Cancelled - PackPals</title>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1'>
    <style>
        body {{ 
            font-family: Arial, sans-serif; 
            text-align: center; 
            background: linear-gradient(135deg, #ff7675 0%, #fd79a8 100%);
            color: white;
            margin: 0;
            padding: 20px;
            min-height: 100vh;
            display: flex;
            flex-direction: column;
            justify-content: center;
        }}
        .container {{
            background: rgba(255,255,255,0.1);
            border-radius: 20px;
            padding: 40px;
            max-width: 500px;
            margin: 0 auto;
            backdrop-filter: blur(10px);
            box-shadow: 0 8px 32px rgba(0,0,0,0.3);
        }}
        .cancel-icon {{
            font-size: 80px;
            margin-bottom: 20px;
            animation: shake 0.5s infinite alternate;
        }}
        @keyframes shake {{
            from {{ transform: translateX(0px); }}
            to {{ transform: translateX(-5px); }}
        }}
        .btn {{
            background: rgba(255,255,255,0.9);
            border: none;
            color: #ff7675;
            padding: 15px 30px;
            border-radius: 30px;
            text-decoration: none;
            display: inline-block;
            margin: 15px;
            font-weight: bold;
            font-size: 16px;
            transition: all 0.3s ease;
            cursor: pointer;
        }}
        .btn:hover {{
            background: white;
            transform: translateY(-2px);
            box-shadow: 0 4px 15px rgba(0,0,0,0.2);
        }}
        .debug {{
            background: rgba(0,0,0,0.2);
            padding: 15px;
            border-radius: 10px;
            font-size: 12px;
            margin-top: 20px;
            font-family: monospace;
            text-align: left;
            max-height: 200px;
            overflow-y: auto;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='cancel-icon'>❌</div>
        <h1>Payment Cancelled</h1>
        <p style='font-size: 18px; margin: 20px 0;'>Your payment was cancelled or failed.</p>
        <p><strong>Order ID:</strong> {orderId ?? "N/A"}</p>
        <p><strong>Order Code:</strong> {orderCode}</p>
        
        <div style='margin-top: 30px;'>
            <button class='btn' onclick='tryOpenApp()'>
                📱 Return to PackPals App
            </button>
            <br>
            <button class='btn' onclick='tryAndroidIntent()' style='background: rgba(255,255,255,0.7);'>
                🤖 Try Android Intent
            </button>
        </div>
        
        <div class='debug' id='debugLog'>
            <strong>🔍 Debug Log:</strong><br>
            Cancel page loaded...<br>
        </div>
        
        <p style='font-size: 12px; opacity: 0.8; margin-top: 20px;'>
            If the app doesn't open, please ensure PackPals is installed and try again.
        </p>
    </div>

    <script>
        var deepLinkUrl = '{deepLinkUrl}';
        var androidIntentUrl = '{androidIntentUrl}';

        function addLog(message) {{
            var log = document.getElementById('debugLog');
            var time = new Date().toLocaleTimeString();
            log.innerHTML += '[' + time + '] ' + message + '<br>';
            log.scrollTop = log.scrollHeight;
            console.log('[PackPals] ' + message);
        }}

        function tryOpenApp() {{
            addLog('🚀 User clicked! Attempting to open app with: ' + deepLinkUrl);
            
            try {{
                window.location.href = deepLinkUrl;
                addLog('✅ Tried window.location.href');
                
                setTimeout(function() {{
                    addLog('⏰ If app did not open, try Android Intent button');
                }}, 2000);
                
            }} catch (e) {{
                addLog('❌ window.location.href failed: ' + e.message);
                
                try {{
                    var link = document.createElement('a');
                    link.href = deepLinkUrl;
                    link.style.display = 'none';
                    document.body.appendChild(link);
                    link.click();
                    document.body.removeChild(link);
                    addLog('✅ Fallback: Tried programmatic click');
                }} catch (e2) {{
                    addLog('❌ All methods failed. Try Android Intent button.');
                }}
            }}
        }}
        
        function tryAndroidIntent() {{
            addLog('🤖 User clicked Android Intent! Trying: ' + androidIntentUrl);
            
            try {{
                window.location.href = androidIntentUrl;
                addLog('✅ Android intent launched');
                
                setTimeout(function() {{
                    addLog('📱 If still not working, check if PackPals app is installed');
                }}, 3000);
                
            }} catch (e) {{
                addLog('❌ Android intent failed: ' + e.message);
                addLog('💡 Make sure PackPals app is installed from APK/Play Store');
            }}
        }}
        
        document.addEventListener('DOMContentLoaded', function() {{
            addLog('📄 Cancel page fully loaded');
            addLog('📱 User Agent: ' + navigator.userAgent);
            addLog('⚠️ Click Return to PackPals App button to open app');
            
            var isMobile = /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent);
            if (isMobile) {{
                addLog('📱 Mobile device detected');
            }} else {{
                addLog('💻 Desktop browser - deeplinks may not work');
            }}
        }});
        
        var startTime = Date.now();
        
        window.addEventListener('blur', function() {{
            addLog('👁️ Page lost focus - app might have opened!');
        }});
        
        window.addEventListener('focus', function() {{
            var timeAway = Date.now() - startTime;
            if (timeAway > 3000) {{
                addLog('👁️ Page regained focus after ' + timeAway + 'ms - returned from app?');
            }}
            startTime = Date.now();
        }});
        
        window.addEventListener('beforeunload', function() {{
            addLog('🚪 Page unloading - app opened successfully!');
        }});
    </script>
</body>
</html>";

            return Content(html, "text/html");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling payment cancel");
            return StatusCode(500, "Internal server error");
        }
    }
}