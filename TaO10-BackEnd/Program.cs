using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TaO10_BackEnd.Models;
using TaO10_BackEnd.Interfaces;
using TaO10_BackEnd.Services;
var builder = WebApplication.CreateBuilder(args);

// Configuration
var configuration = builder.Configuration;

// CORS - allow Angular dev origin
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:4200" };

builder.Services.AddCors(options =>
{
 options.AddPolicy("AllowAll", policy =>
 {
 policy.WithOrigins(allowedOrigins)
 .AllowAnyHeader()
 .AllowAnyMethod()
 .AllowCredentials(); // only if you use cookies; otherwise you can omit AllowCredentials
 });
});

// Add services to the container.
builder.Services.AddControllers()
 .AddJsonOptions(options =>
 {
 options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
 });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
 options.UseNpgsql(builder.Configuration.GetConnectionString("MyCnn")));

// JWT helper and auth service
builder.Services.AddSingleton<JwtHelper>();

////DI services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Configure PayOS
var payOsConfig = builder.Configuration.GetSection("PayOS");
PayOS.PayOSClient payOS = new PayOS.PayOSClient(
    payOsConfig["ClientId"] ?? "",
    payOsConfig["ApiKey"] ?? "",
    payOsConfig["ChecksumKey"] ?? ""
);
builder.Services.AddSingleton(payOS);

// Register payment provider abstraction
builder.Services.AddSingleton<IPaymentProvider, PayOSPaymentProvider>();

// real-time
builder.Services.AddSignalR();

// Configure JWT authentication
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(options =>
{
 options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
 options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
 options.TokenValidationParameters = new TokenValidationParameters
 {
 ValidateIssuer = true,
 ValidateAudience = true,
 ValidIssuer = jwtIssuer,
 ValidAudience = jwtAudience,
 ValidateLifetime = true,
 IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
 ValidateIssuerSigningKey = true
 };

 options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
 {
 OnMessageReceived = context =>
 {
 var accessToken = context.Request.Query["access_token"];
 var path = context.Request.Path;
 if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
 {
 context.Token = accessToken;
 }
 return Task.CompletedTask;
 }
 };
});

var app = builder.Build();

// Seed data
using (var scope = app.Services.CreateScope())
{
 var services = scope.ServiceProvider;
 try
 {
 var context = services.GetRequiredService<AppDbContext>();
 await TaO10_BackEnd.Helpers.DbSeeder.SeedAsync(context);
 }
 catch (Exception ex)
 {
 var logger = services.GetRequiredService<ILogger<Program>>();
 logger.LogError(ex, "An error occurred while seeding the database.");
 }
}

if (app.Environment.IsDevelopment())
{
 app.UseSwagger();
 app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
 app.UseHttpsRedirection();
}

// Use CORS before authentication/authorization
app.UseCors("AllowAll");

app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();
app.MapHub<TaO10_BackEnd.Hubs.NotificationHub>("/hubs/notification");

app.Run();
