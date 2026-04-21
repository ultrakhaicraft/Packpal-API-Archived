using Packpal.BLL;
using Packpal.DI;
using Packpal.BLL.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication(builder.Configuration);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();

// Add SignalR
builder.Services.AddSignalR();

/*// Configure SignalR notifications (override default implementation)
builder.Services.AddSignalRNotifications();*/

// Set the minimum level to Debug
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// Configure Entity Framework warnings
builder.Services.Configure<LoggerFilterOptions>(options =>
{
    options.Rules.Add(new LoggerFilterRule(
        "Microsoft.EntityFrameworkCore.Query",
        "Microsoft.EntityFrameworkCore.Query",
        LogLevel.Warning,
        (_, _, _) => false
    ));
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Packpal API V1");
	c.DocumentTitle = "Packpal API Documentation";
	c.RoutePrefix = "swagger";

});

// Use SignalR-specific CORS policy
app.UseCors("SignalRCors");

app.UseRouting();

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Map SignalR Hubs
app.MapHub<KeeperNotificationHub>("/hubs/keeper-notifications");
app.MapHub<StaffNotificationHub>("/signalrhub");

app.Run();
