using Microsoft.EntityFrameworkCore;
using ST10439147_PRO7311_POE_Back.Data;
using ST10439147_PRO7311_POE_Back.DTOs;
using ST10439147_PRO7311_POE_Back.Hubs;
using ST10439147_PRO7311_POE_Back.Services;

var builder = WebApplication.CreateBuilder(args);

var dataFolder = Path.Combine(builder.Environment.ContentRootPath, "Data");
Directory.CreateDirectory(dataFolder);
var mdfPath = Path.Combine(dataFolder, "ST10439147_PRO7311_POE.mdf");
var connectionString = $"Server=(localdb)\\mssqllocaldb;" +
                       $"AttachDbFilename={mdfPath};" +
                       $"Database=ST10439147_GLMS_DB;" +
                       $"Integrated Security=True;" +
                       $"MultipleActiveResultSets=true";

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddSignalR();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<LoginDTO>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<ICurrencyConversionStrategy, OpenExchangeStrategy>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorClient", policy =>
        policy.WithOrigins("https://localhost:7181")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("BlazorClient");
app.UseHttpsRedirection();
app.MapControllers();
app.MapHub<NotificationHub>("/notificationHub");

app.Run();