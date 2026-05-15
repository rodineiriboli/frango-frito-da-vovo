using FrangoFrito.Api.Middleware;
using FrangoFrito.Application.Security;
using FrangoFrito.Infrastructure;
using FrangoFrito.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "frango_frito_auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;

    options.Events = new CookieAuthenticationEvents
    {
        OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        },
        OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AppPolicies.AdminOnly, policy => policy.RequireRole(AppRoles.Admin));
    options.AddPolicy(AppPolicies.Backoffice, policy => policy.RequireRole(AppRoles.Admin, AppRoles.Attendant, AppRoles.Kitchen, AppRoles.Courier));
    options.AddPolicy(AppPolicies.OrderCreation, policy => policy.RequireRole(AppRoles.Admin, AppRoles.Attendant));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("Spa", policy =>
    {
        var allowedOrigins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .GetChildren()
            .Select(section => section.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToArray();

        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins!)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
    });
});

var app = builder.Build();

app.UseMiddleware<ApiExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("Spa");
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

if (!app.Environment.IsEnvironment("Testing"))
{
    await app.Services.InitializeDatabaseAsync();
}

app.Run();

public partial class Program;
