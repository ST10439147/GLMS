using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using ST10439147_PRO7311_POE_Back.Data;
using ST10439147_PRO7311_POE_Back.DTOs;
using ST10439147_PRO7311_POE_Back.Hubs;
using ST10439147_PRO7311_POE_Back.Services;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("DefaultConnection is not configured.");

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddSignalR();
builder.Services.AddScoped<LoginDTO>();
builder.Services.AddHttpClient();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<ICurrencyConversionStrategy, OpenExchangeStrategy>();

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is not configured.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

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

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token. Obtained from POST /api/auth/login."
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

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Swagger available in all environments so the API is always testable
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "TechMove API v1");
    options.RoutePrefix = "swagger";
});

app.UseCors("BlazorClient");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<NotificationHub>("/notificationHub");

app.Run();
