using GLMS.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Extensions.Http;
using ST10439147_PRO7311_POE.Components;
using ST10439147_PRO7311_POE.Data;
using ST10439147_PRO7311_POE.DTOs;
using ST10439147_PRO7311_POE.Hubs;
using ST10439147_PRO7311_POE.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSignalR();
builder.Services.AddScoped<NotificationService>();

// allow data to be shared between pages
builder.Services.AddScoped<LoginDTO>();
builder.Services.AddScoped<UserState>();

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Required by OpenExchangeStrategy (IHttpClientFactory) and any named clients
builder.Services.AddHttpClient();

// ── Register the handler that stamps X-Access-Level on every API request ──
builder.Services.AddScoped<AccessLevelHandler>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<ICurrencyConversionStrategy, OpenExchangeStrategy>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);

// ── Replace the bare HttpClient registration with one that uses the handler ──
builder.Services.AddScoped(sp =>
{
    var nav = sp.GetService<NavigationManager>();
    var baseUrl = nav?.BaseUri ?? "https://localhost:7181/";

    // Pull the handler from DI (it has access to UserState)
    var handler = sp.GetRequiredService<AccessLevelHandler>();
    handler.InnerHandler = new HttpClientHandler();

    return new HttpClient(handler) { BaseAddress = new Uri(baseUrl) };
});

var app = builder.Build();

// ── Pre-warm the database connection pool ────────────────────────────────────
// The first SQL Server connection after app start has a 2-3 second cold-start
// cost (TCP handshake, TLS, EF model validation). By opening a throwaway
// connection here — while the app is starting up in the background — the pool
// is ready before the first user clicks Login, so their request is instant.
_ = Task.Run(async () =>
{
    try
    {
        await using var ctx = app.Services
            .GetRequiredService<IDbContextFactory<AppDbContext>>()
            .CreateDbContext();
        await ctx.Database.ExecuteSqlRawAsync("SELECT 1");
    }
    catch
    {
        // Swallow — if the DB is unavailable at startup the app still loads;
        // the error will surface naturally when the user tries to log in.
    }
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.MapControllers();

app.MapHub<NotificationHub>("/notificationHub");

app.Run();