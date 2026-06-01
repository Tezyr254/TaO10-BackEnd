using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TaO10_BackEnd.Models;
using TaO10_BackEnd.Interfaces;
using TaO10_BackEnd.Services;
using TaO10_BackEnd.Repositories;
using TaO10_BackEnd.Mappers;
using TaO10_BackEnd.Middleware;

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
builder.Services.AddControllers();
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

// Repository DI
builder.Services.AddScoped<IStatusRepository, StatusRepository>();
builder.Services.AddScoped<IExamRepository, ExamRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<IUserExamAttemptRepository, UserExamAttemptRepository>();
builder.Services.AddScoped<IUserAnswerRepository, UserAnswerRepository>();

// Mapper DI
builder.Services.AddScoped<IExamMapper, ExamMapper>();

// Service DI
builder.Services.AddScoped<IExamService, ExamService>();
builder.Services.AddScoped<IUserExamAttemptService, UserExamAttemptService>();

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
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Use CORS before authentication/authorization
app.UseCors("AllowAll");

// Global exception middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();

app.Run();
