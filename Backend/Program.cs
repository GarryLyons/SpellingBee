using Backend.Endpoints;
using Backend.Middleware;
using Backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000") // Adjust if needed
              .SetIsOriginAllowed(origin => true) // Allow any origin in dev to fix possible mismatches
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Authentication:Cognito:Authority"];
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateLifetime = true,
            ValidateAudience = false, // Essential for Cognito Access Tokens
            ValidateIssuerSigningKey = true
        };
        
        // Debug logging
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Auth Failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnMessageReceived = context =>
            {
                // Debug log to confirm token is arriving
                var token = context.Request.Headers["Authorization"].FirstOrDefault();
                if (!string.IsNullOrEmpty(token))
                {
                   Console.WriteLine($"Token received (len: {token.Length}): {token.Substring(0, Math.Min(token.Length, 20))}...");
                }
                else
                {
                   Console.WriteLine("No Authorization header received.");
                }
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("Auth Success: Token validated.");
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Console.WriteLine($"Auth Challenge: {context.Error} - {context.ErrorDescription}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// Domain Services
builder.Services.AddSingleton<WordBankRepository>();
builder.Services.AddSingleton<PracticeEngine>();
builder.Services.AddSingleton<PracticeSessionStore>();

var app = builder.Build();

// 2. Configure Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Simple request logging for debugging
app.Use(async (context, next) =>
{
    Console.WriteLine($"Incoming Request: {context.Request.Method} {context.Request.Path}");
    await next();
});

// app.UseHttpsRedirection(); // Keep commented out for local http dev

app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

// 3. Define Endpoints (Fixes CS0103)
var api = app.MapGroup("/api");

// Map your endpoints - ensure you have the correct using statements for these extensions
api.MapWordsEndpoints();
api.MapPracticeSessionEndpoints();

app.Run();
