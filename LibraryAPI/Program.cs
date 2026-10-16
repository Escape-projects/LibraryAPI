using LibraryAPI;
using LibraryAPI.Data;
using LibraryAPI.Middleware;
using LibraryAPI.OpenApi;
using LibraryAPI.Services.API;
using LibraryAPI.Services.Jwt;
using LibraryAPI.Services.Redis;
using LibraryAPI.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using StackExchange.Redis;
using System.Text;

// Stage 1: Bootstrap logger — ловит ошибки до builder.Build()
Log.Logger = new LoggerConfiguration().MinimumLevel.Override("Microsoft", LogEventLevel.Warning).WriteTo.Console().CreateBootstrapLogger();

try
{
    Log.Information("Starting application");

    var builder = WebApplication.CreateBuilder(args);

    // Stage 2: Полноценный logger с DI и конфигурацией из appsettings.json
    builder.Services.AddSerilog((services, lc) => lc.ReadFrom.Configuration(builder.Configuration).ReadFrom.Services(services).Enrich.FromLogContext());

    // Add services to the container.
    builder.Services.AddControllers();
    builder.Services.AddDbContext<LibraryContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    var redisConnectionString = builder.Configuration["Redis:ConnectionString"] ?? throw new InvalidOperationException("Redis connection string is not configured");
    var options = ConfigurationOptions.Parse(redisConnectionString);
    options.AbortOnConnectFail = false;
    options.ConnectTimeout = 1000;
    options.ConnectRetry = 1;
    options.AsyncTimeout = 500;
    options.SyncTimeout = 500;
    builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(options));
    builder.Services.AddSingleton<ICacheService, RedisCacheService>();

    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IBookService, BookService>();
    builder.Services.AddScoped<IAuthorService, AuthorService>();
    builder.Services.AddScoped<IAccountService, AccountService>();
    builder.Services.AddAutoMapper(cfg => { }, typeof(Program).Assembly);
    
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    builder.Services.AddOptions<JwtSettings>().Bind(builder.Configuration.GetSection("JwtSettings"))
    .Validate(settings => !string.IsNullOrWhiteSpace(settings.SecretKey), "JwtSettings:SecretKey is not configured. Locally: user-secrets. In Docker: JwtSettings__SecretKey.")
    .Validate(settings => settings.SecretKey.Length >= 32, "JwtSettings:SecretKey must be at least 32 characters (256 bits for HMAC-SHA256).")
    .Validate(settings => settings.LifetimeMinutes > 0, "JwtSettings:LifetimeMinutes must be positive.")
    .ValidateOnStart();
    builder.Services.AddScoped<IJwtService, JwtService>();
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>() ?? throw new InvalidOperationException("JwtSettings section is missing in configuration.");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
        };
    });
    builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
    builder.Services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();

    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi(options => { options.AddDocumentTransformer<SecuritySchemeTransformer>(); });

    var app = builder.Build();

    // Request logging middleware — логирует каждый HTTP-запрос
    app.UseSerilogRequestLogging();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
    }

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<LibraryContext>();
        db.Database.Migrate();
    }

    if (app.Environment.IsDevelopment())
    {
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LibraryContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            DatabaseSeeder.Seed(db, hasher, logger);
        }
    }

    app.UseExceptionHandler();
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync(); //Сбросить все буферы логов
}
