using FirebaseAdmin;
using Google.Apis.Auth.OAuth2; // Required for GoogleCredential
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Net.payOS;
using Packpal.BLL;
using Packpal.BLL.Interface;
using Packpal.BLL.Services;
using Packpal.BLL.Utilities;
using Packpal.DAL.Context;
using Packpal.DAL.ModelViews;
using System.Reflection;
using System.Text;


namespace Packpal.DI
{
    public static class DependencyInjection
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.ConfigSwagger();

            services.AddAuthenJwt(configuration);

            services.AddDatabase(configuration);
            services.AddServices(configuration);
            services.ConfigRoute();         
            services.ConfigCors();
            services.AddFirebaseSDK(configuration);
			services.AddHttpClient<IPayoutService, PayoutService>();
		}
        public static void ConfigCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                               .AllowAnyHeader()
                               .AllowAnyMethod();
                    });

                // Add specific policy for SignalR - Allow all origins for development
                options.AddPolicy("SignalRCors",
                    builder =>
                    {
                        builder.SetIsOriginAllowed(origin => true) // Allow all origins for React Native
                               .AllowAnyHeader()
                               .AllowAnyMethod()
                               .AllowCredentials(); // Required for SignalR
                    });
            });
        }
        public static void ConfigRoute(this IServiceCollection services)
        {
            services.Configure<RouteOptions>(options =>
            {
                options.LowercaseUrls = true;
            });
        }
        public static void AddAuthenJwt(this IServiceCollection services, IConfiguration configuration)
        {
            // Bind JwtSettings from appsettings.json
            services.Configure<JwtModel>(configuration.GetSection("JwtSettings"));

            // Add Authentication + JWT Bearer
            var jwtSection = configuration.GetSection("JwtSettings");
            var jwtConfig = jwtSection.Get<JwtModel>();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = true;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtConfig?.ValidIssuer,
                    ValidAudiences = new[]
                    {
                        jwtConfig?.ValidAudience,
                        jwtConfig?.ValidTester
                    },
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig?.SecretKey ?? ""))

                };
            });
        }
        public static void ConfigSwagger(this IServiceCollection services)
        {
            // config swagger
            services.AddSwaggerGen(option =>
            {
				option.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "API"

                });

				// Get the XML comment file path
				var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

				// Include the XML comments
				option.IncludeXmlComments(xmlPath);

				// Thêm JWT Bearer Token vào Swagger
                option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	            {
		            In = ParameterLocation.Header,
		            Description = "Please enter a valid token",
		            Name = "Authorization",
		            Type = SecuritySchemeType.Http,
		            BearerFormat = "JWT",
		            Scheme = "Bearer"
	            });

                option.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] { }
                    }
                });

				option.UseInlineDefinitionsForEnums();
			});
        }
        public static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<PackpalDbContext>(options =>
            {
                //options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
            });
        }

        public static void AddFirebaseSDK(this IServiceCollection services, IConfiguration configuration)
        {
			try
			{
				// Initialize the Firebase Admin SDK.

				var config = configuration.GetSection("Firebase");
				if (config == null || string.IsNullOrEmpty(config["CredentialsPath"]) || string.IsNullOrEmpty(config["BucketName"]))
				{
					throw new AppException("Firebase configuration is missing or incomplete.");
				}

                
				services.AddSingleton<IFirebaseStorageService>(provider =>
				{
					var config = provider.GetRequiredService<IConfiguration>();
					var credentialsPath = config["Firebase:CredentialsPath"]!;
					var bucketName = config["Firebase:BucketName"]!;
					return new FirebaseStorageService(credentialsPath, bucketName);
				});

				services.AddSingleton<PayOS>(provider =>
				{
					var config = provider.GetRequiredService<IConfiguration>().GetSection("PayOSConfig").Get<PayOSConfig>()!;
					return new PayOS(config.ClientId, config.ApiKey, config.SecretKey);
				});

				Console.WriteLine("Firebase Admin SDK initialized successfully!");

			}
			catch (Exception ex)
			{
				Console.Error.WriteLine("Error initializing Firebase Admin SDK: " + ex.Message);	
                Environment.Exit(1); // Exit the application if Firebase initialization fails
			}
		}

        /*/// <summary>
        /// Configure SignalR notifications
        /// </summary>
        /// <param name="services"></param>
        public static void AddSignalRNotifications(this IServiceCollection services)
        {
            // Replace the default notification service with SignalR implementation
            services.AddScoped<INotificationService, SignalRNotificationService>();
        }*/
    }
}
