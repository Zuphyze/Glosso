using Glosso.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Glosso.Data;
using Glosso.Services;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Razor components and interactive server components
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add services for user and auth state
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AuthStateService>();

// Configure Antiforgery
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN"; // Set the header name for the token
});

// Add HTTP Client for API calls
builder.Services.AddHttpClient("GlossoClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["BaseAddress"]); // Set your base address here
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    UseCookies = true, // Ensure cookies are enabled
    CookieContainer = new CookieContainer() // This stores cookies for the session
});

// Add Controllers for API support
builder.Services.AddControllers();

// CORS configuration to allow requests from Blazor app
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorApp", builder =>
    {
        builder.WithOrigins("https://localhost:7238") // Set your Blazor app URL here
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials(); // Allow credentials for authentication
    });
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting(); // Ensure routing is before CORS
app.UseCors("AllowBlazorApp"); // CORS middleware
app.UseAuthentication(); // If using authentication
app.UseAuthorization();
app.UseAntiforgery(); // Enable antiforgery token validation

// Map Razor components
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Map API controllers (important for API routes to work)
app.MapControllers();

app.Run();
