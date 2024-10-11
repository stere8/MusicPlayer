using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using SonorousAPI.Controllers;
using SonorousAPI.Models;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    WebRootPath = "uploads" // Set the root path to "uploads" when the builder is created
});

// Add services to the container.
builder.Services.AddControllers();

// Add DbContext to use SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Kestrel server options (for handling larger file uploads)
builder.WebHost.ConfigureKestrel(serverOptions => { serverOptions.Limits.MaxRequestBodySize = null; });

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", policyBuilder =>
    {
        policyBuilder.WithOrigins("http://localhost:3000")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});
// Add SignalR for real-time communication
builder.Services.AddSignalR();

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = null; // Allow large request bodies
});

// Build the app
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Use CORS middleware (Allow all origins for now)
app.UseCors("AllowSpecificOrigin");

// Serve static files from the "uploads" folder and disable caching
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Cache-Control", "no-store"); // Disable caching for static files
    }
});

app.UseRouting();
app.UseHttpsRedirection();

app.UseAuthorization();

// Map controllers and hubs
app.MapControllers();
app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<AudioStreamHub>("/audioStreamHub");
    endpoints.MapControllers();
});

// Run the application
app.Run();
