using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Packpal.BLL.Interface;
using Packpal.BLL.Services;
using Packpal.BLL.Utilities;
using Packpal.Contract.Repositories.Interface;
using Packpal.Repositories.Repositories;
using System.Reflection;


namespace Packpal.BLL;

public static class DependencyInjection
{
    public static void AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
		
		services.AddRepository();
        services.AddAutoMapper();
        services.AddServices(configuration);
    }

    public static void AddRepository(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
    }

    private static void AddAutoMapper(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
    }

    public static void AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddLogging();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IJwtUtils, JwtUtils>();
        services.AddScoped<IStorageService, StorageService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITransactionService,TransactionService>();
        services.AddScoped<IPayoutService,PayoutService>();
        services.AddScoped<IOrderDetailService, OrderDetailService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IRatingService, RatingService>();
        services.AddScoped<ISizeService, SizeService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IRequestService, RequestService>();
        services.AddScoped<INotificationService, SignalRNotificationService>();
    }
}
