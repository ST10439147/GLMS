using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using ST10439147_PRO7311_POE_Back.Data;
using ST10439147_PRO7311_POE_Back.DTOs;
using ST10439147_PRO7311_POE_Back.Hubs;
using ST10439147_PRO7311_POE_Back.Services;
using System.Reflection;

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
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TechMove Contract Management API",
        Version = "v1",
        Description =
            "REST API for managing contracts, clients, service requests, and currency conversion. " +
            "Pass the caller's access level in the X-Access-Level request header " +
            "(1 = Admin, 2 = Manager, 3 = Viewer)."
    });

    // Pull in /// XML summary comments for self-documenting endpoints
    var xmlPath = Path.Combine(AppContext.BaseDirectory,
        $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);

    // Document the custom header-based access-level scheme
    options.AddSecurityDefinition("AccessLevel", new OpenApiSecurityScheme
    {
        Name = "X-Access-Level",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Description = "Access level: 1 = Admin, 2 = Manager, 3 = Viewer"
    });

});

builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorClient", policy =>
        policy.WithOrigins("https://localhost:7181")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

var app = builder.Build();

// Swagger available in all environments so the API is always testable
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "TechMove API v1");
    options.RoutePrefix = "swagger";
});

app.UseCors("BlazorClient");
app.UseHttpsRedirection();
app.MapControllers();
app.MapHub<NotificationHub>("/notificationHub");

app.Run();
