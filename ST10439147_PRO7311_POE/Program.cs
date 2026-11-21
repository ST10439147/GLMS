///References
///Claude: Used to set up the HttpClient registration so the AccessLevelHandler is applied to all API requests,
///and on how to handle errors gracefully when the API is unavailable at startup.
///Link: https://claude.ai/chat/894f0b1b-482f-4d52-890c-f74b49b5173c
///Secure ASP.NET Core Blazor WebAssembly | Microsoft Learn: https://learn.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/?view=aspnetcore-10.0
///Implement HTTP call retries with exponential backoff with Polly - .NET | Microsoft Learn: https://learn.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/implement-http-call-retries-exponential-backoff-polly
///.NET dependency injection: https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection/overview
///Overview of ASP.NET Core SignalR: https://learn.microsoft.com/en-us/aspnet/core/signalr/introduction?view=aspnetcore-10.0

using ST10439147_PRO7311_POE.Services;
using Microsoft.AspNetCore.Components;
using ST10439147_PRO7311_POE.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSignalR();
builder.Services.AddScoped<UserState>();
builder.Services.AddScoped<AccessLevelHandler>();

var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7055";

builder.Services.AddScoped(sp =>
{
    var handler = sp.GetRequiredService<AccessLevelHandler>();
    handler.InnerHandler = new HttpClientHandler();

    return new HttpClient(handler) { BaseAddress = new Uri(apiBaseUrl + "/") };
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
//--------------------------------------DDDDDoooooo End of File DDDDDoooooo--------------------------------------
