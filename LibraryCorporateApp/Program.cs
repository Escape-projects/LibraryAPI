using LibraryCorporateApp.Exceptions;
using LibraryCorporateApp.Menus;
using LibraryCorporateApp.Middleware;
using LibraryCorporateApp.Models;
using LibraryCorporateApp.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using System.Net;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSingleton<AppStorage>();
builder.Services.AddTransient<AuthHeaderHandler>();
builder.Logging.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);

builder.Services.AddScoped<IBookClientService, BookClientService>();
builder.Services.AddScoped<IAuthorClientService, AuthorClientService>();
builder.Services.AddScoped<IAccountClientService, AccountClientService>();

builder.Services.AddScoped<LoginMenu>();
builder.Services.AddScoped<LibrarianMenu>();
builder.Services.AddScoped<AdminMenu>();
builder.Services.AddScoped<BooksMenu>();
builder.Services.AddScoped<AuthorsMenu>();
builder.Services.AddScoped<AccountsMenu>();

builder.Services.AddHttpClient<ApiClient>(client => client.BaseAddress = new Uri("https://localhost:7119/"))
    .AddHttpMessageHandler<AuthHeaderHandler>().AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(attempt)));

var host = builder.Build();
using var scope = host.Services.CreateScope();

try 
{
    var loginMenu = scope.ServiceProvider.GetRequiredService<LoginMenu>();
    await loginMenu.Run();
}
catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
{
    Console.WriteLine("***сессия истекла, войдите заново");
}
catch (ApiException ex)
{
    Console.WriteLine($"***ошибка сервера: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"***неожиданная ошибка: {ex.Message}");
}