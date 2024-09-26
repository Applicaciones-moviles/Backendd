using System.Reflection;
using System.Text;
using DriveSafe.Application.Publishing.CommandServices;
using DriveSafe.Application.QueryServices;
using DriveSafe.Application.Security.CommandServices;
using DriveSafe.Domain.Publishing.Repositories;
using DriveSafe.Domain.Publishing.Services;
using DriveSafe.Domain.Security.Services;
using DriveSafe.Infraestructure.Publishing.Persistence;
using DriveSafe.Infraestructure.Shared.Context;
using DriveSafe.Presentation.Mapper;
using DriveSafe.Presentation.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllPolicy", 
        policy => policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// Load configuration from appsettings.json
var configuration = new ConfigurationBuilder()
    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables() // Agregar esta línea
    .Build();
builder.Services.AddSingleton(configuration);

foreach (var c in configuration.AsEnumerable())
{
    Console.WriteLine($"{c.Key}: {c.Value}");
}

// Add services to the container
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
});

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "DriveSafe API",
        Description = "An ASP.NET Core Web API for the DriveSafe application",
        TermsOfService = new Uri("https://example.com/terms"),
        Contact = new OpenApiContact
        {
            Name = "Example Contact",
            Url = new Uri("https://example.com/contact")
        },
        License = new OpenApiLicense
        {
            Name = "Example License",
            Url = new Uri("https://example.com/license")
        }
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            },
            Array.Empty<string>()
        }
    });
});

// Register services
builder.Services.AddScoped<IMaintenanceRepository, MaintenanceRepository>();
builder.Services.AddScoped<IMaintenanceCommandService, MaintenanceCommandService>();
builder.Services.AddScoped<IMaintenanceQueryService, MaintenanceQueryService>();
builder.Services.AddScoped<IRentRepository, RentRepository>();
builder.Services.AddScoped<IRentCommandService, RentCommandService>();
builder.Services.AddScoped<IRentQueryService, RentQueryService>();
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IVehicleCommandService, VehicleCommandService>();
builder.Services.AddScoped<IVehicleQueryService, VehicleQueryService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserCommandService, UserCommandService>();
builder.Services.AddScoped<IUserQueryService, UserQueryService>();
builder.Services.AddScoped<IEncryptService, EncryptService>();
builder.Services.AddScoped<ITokenService, TokenService>();

// Configure authentication
var jwtSecret = configuration["APPSETTINGS_SECRET"] ?? configuration["AppSettings:Secret"];
if (string.IsNullOrEmpty(jwtSecret))
{
    throw new InvalidOperationException("JWT secret is not configured. Please set APPSETTINGS_SECRET environment variable or AppSettings:Secret in appsettings.json");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => 
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

Console.WriteLine($"APPSETTINGS_SECRET: {configuration["APPSETTINGS_SECRET"]}");


// Add AutoMapper
builder.Services.AddAutoMapper(
    typeof(RequestToModel),
    typeof(ModelToRequest),
    typeof(ModelToResponse));

// Use the correct connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<DriveSafeDBContext>(
    dbContextOptions =>
    {
        dbContextOptions.UseMySql(connectionString,
            ServerVersion.AutoDetect(connectionString)
        );
    });

// Add Application Insights telemetry
builder.Services.AddApplicationInsightsTelemetry();

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;
        await context.Response.WriteAsJsonAsync(new { error = exception?.Message });
        Console.WriteLine($"Unhandled exception: {exception}");
    });
});

var dbConnectionString = configuration.GetConnectionString("DefaultConnection") ?? 
    Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");

if (string.IsNullOrEmpty(dbConnectionString))
{
    throw new InvalidOperationException("Database connection string is not configured.");
}

// Ensure database is created
using (var scope = app.Services.CreateScope())
using (var context = scope.ServiceProvider.GetService<DriveSafeDBContext>())
{
    context.Database.EnsureCreated();
}

app.MapGet("/debug", (IConfiguration config) =>
{
    var debugInfo = new
    {
        EnvironmentVariables = Environment.GetEnvironmentVariables(),
        ConfigurationValues = config.AsEnumerable().ToDictionary(x => x.Key, x => x.Value),
        DatabaseConnectionString = config.GetConnectionString("DefaultConnection")
    };
    return Results.Json(debugInfo);
});

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseCors("AllowAllPolicy"); 
//app.UseHttpsRedirection();

//app.UseAuthentication();
//app.UseAuthorization();

app.UseMiddleware<ErrorHandlerMiddleware>();

app.UseMiddleware<AuthenticationMiddleware>();

app.MapControllers();

app.Run();
