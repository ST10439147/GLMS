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

//builder.Services.AddSignalR(); // per page
builder.Services.AddScoped<NotificationService>(); //SignalR connection exists once for the entire app

// allow data to be shared between pages
builder.Services.AddSingleton<LoginDTO>();
builder.Services.AddScoped<UserState>();

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer("DefaultConnection"));

builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddControllers();

builder.Services.AddScoped(sp =>
{
    var nav = sp.GetRequiredService<NavigationManager>();
    return new HttpClient
    {
        BaseAddress = new Uri(nav.BaseUri)
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
