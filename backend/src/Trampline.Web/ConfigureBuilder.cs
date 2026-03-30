using FluentValidation;
using Trampline.Application.Services;
using Trampline.Application.Services.Employees;
using Trampline.Application.Services.Events;
using Trampline.Application.Services.IO;
using Trampline.Application.Services.Mentorships;
using Trampline.Application.Validators;
using Trampline.Core.Options;
using Trampline.Core.Repositories;
using Trampline.Core.Storage;
using Trampline.Infrastructure.Postgres.Repositories;
using Trampline.Infrastructure.Redis.Repositories;
using Trampline.Shared.Services;

namespace Trampline.Web;

public static class ConfigureBuilder
{
    extension(IServiceCollection services)
    {
        public IServiceCollection ConfigureServices()
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IInfoService, InfoService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IWorkerService, WorkerService>();
            services.AddScoped<IMediaService, MediaService>();

            var storageProvider = Environment.GetEnvironmentVariable("STORAGE_PROVIDER") ?? "local";
            if (storageProvider.Equals("minio", StringComparison.OrdinalIgnoreCase))
                services.AddSingleton<IStorageService, MinioStorageService>();
            else
                services.AddSingleton<IStorageService, LocalStorageService>();
            services.AddScoped<IJobService, JobService>();
            services.AddScoped<IEventService, EventService>();
            services.AddScoped<IDaDataService, DaDataService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IAuditService, AuditService>();
            services.AddScoped<IMentorshipService, MentorshipService>();
            services.AddSingleton<ITotpService, TotpService>();
            return services;
        }

        public IServiceCollection ConfigureRepositories()
        {
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserSessionRepository, UserSessionRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<IWorkerRepository, WorkerRepository>();
            services.AddScoped<IJobRepository, JobRepository>();
            services.AddScoped<IJobApplicationRepository, JobApplicationRepository>();
            services.AddScoped<IEventApplicationRepository, EventApplicationRepository>();
            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IContactRepository, ContactRepository>();
            services.AddScoped<IFavoriteRepository, FavoriteRepository>();
            services.AddScoped<IReviewRepository, ReviewRepository>();
            services.AddScoped<ITagRepository, TagRepository>();
            services.AddScoped<IRecommendationRepository, RecommendationRepository>();
            services.AddScoped<IAuditLogRepository, AuditLogRepository>();
            services.AddScoped<IMentorshipRepository, MentorshipRepository>();
            services.AddScoped<IMentorshipApplicationRepository, MentorshipApplicationRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IMapRepository, MapRepository>();
            return services;
        }

        public IServiceCollection ConfigureValidators()
        {
            services.AddValidatorsFromAssemblyContaining<RegisterValidator>();
            return services;
        }

        public IServiceCollection ConfigureOptions(ConfigurationManager manager)
        {
            services.Configure<JwtOption>(manager.GetSection("Jwt"));
            services.Configure<DaDataOption>(manager.GetSection("DaData"));
            services.Configure<SmtpOption>(manager.GetSection("Smtp"));
            return services;
        }
    }
}