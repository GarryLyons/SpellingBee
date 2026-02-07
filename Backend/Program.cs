using Backend.Endpoints;
using Backend.Middleware;
using Backend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddSingleton<WordBankRepository>();
builder.Services.AddSingleton<PracticeEngine>();
builder.Services.AddSingleton<PracticeSessionStore>();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseCors("Frontend");

var api = app.MapGroup("/api");
api.MapWordsEndpoints();
api.MapPracticeSessionEndpoints();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
