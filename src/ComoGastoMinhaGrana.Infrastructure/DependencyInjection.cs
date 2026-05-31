using ComoGastoMinhaGrana.Application.Common.Interfaces;
using ComoGastoMinhaGrana.Domain.Entities;
using ComoGastoMinhaGrana.Infrastructure.Consumers;
using ComoGastoMinhaGrana.Infrastructure.Persistence;
using ComoGastoMinhaGrana.Infrastructure.Persistence.Repositories;
using ComoGastoMinhaGrana.Infrastructure.Services;
using ComoGastoMinhaGrana.Infrastructure.Services.Extraction;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ComoGastoMinhaGrana.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        // --- Banco de dados ---
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // --- Identity ---
        services.AddIdentity<User, IdentityRole<Guid>>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = "CGMG.Auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
            options.Cookie.SecurePolicy = environment.IsDevelopment()
                ? Microsoft.AspNetCore.Http.CookieSecurePolicy.None
                : Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
            options.ExpireTimeSpan = TimeSpan.FromDays(30);
            options.SlidingExpiration = true;
            options.Events.OnRedirectToLogin = ctx =>
            {
                ctx.Response.StatusCode = 401;
                return Task.CompletedTask;
            };
        });

        // --- Repositórios ---
        services.AddScoped<IFinancialStatementRepository, FinancialStatementRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICategoryRuleRepository, CategoryRuleRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();

        // --- Extratores de documento ---
        services.AddTransient<IDocumentTextExtractor, PdfTextExtractor>();
        services.AddTransient<IDocumentTextExtractor, TxtTextExtractor>();
        services.AddTransient<IDocumentTextExtractor, ExcelTextExtractor>();
        services.AddTransient<IDocumentTextExtractor, ImageTextExtractor>();
        services.AddScoped<IDocumentExtractorFactory, DocumentExtractorFactory>();

        // --- Sanitização e cache ---
        services.AddSingleton<ISanitizerService, SanitizerService>();
        services.AddScoped<IAnalysisCacheService, AnalysisCacheService>();
        services.AddScoped<IReportCacheService, ReportCacheService>();
        services.AddScoped<IExportService, ExportService>();

        // --- Redis ---
        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnection))
            services.AddStackExchangeRedisCache(opts => opts.Configuration = redisConnection);
        else
            services.AddDistributedMemoryCache(); // fallback em dev sem Redis

        // --- IA (DeepSeek via OpenRouter) ---
        services.AddHttpClient<IAIService, DeepSeekService>();

        // --- Mensageria ---
        if (environment.IsDevelopment())
        {
            // Em dev, o bus InMemory processa mensagens no mesmo processo (sem RabbitMQ)
            services.AddMassTransit(x =>
            {
                x.AddConsumer<ProcessStatementConsumer>(cfg =>
                    cfg.UseMessageRetry(r =>
                        r.Exponential(3, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(10))));

                x.UsingInMemory((ctx, cfg) => cfg.ConfigureEndpoints(ctx));
            });
        }
        else
        {
            services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((ctx, cfg) =>
                {
                    var rabbit = configuration.GetSection("RabbitMQ");
                    cfg.Host(rabbit["Host"] ?? "localhost", h =>
                    {
                        h.Username(rabbit["Username"] ?? "guest");
                        h.Password(rabbit["Password"] ?? "guest");
                    });
                    cfg.ConfigureEndpoints(ctx);
                });
            });
        }
        services.AddScoped<IMessagePublisher, MessagePublisher>();

        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

        return services;
    }
}
