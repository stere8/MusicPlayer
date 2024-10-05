using SonorousAPI.Controllers;
using SonorousAPI.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.WebHost.ConfigureKestrel(serverOptions => { serverOptions.Limits.MaxRequestBodySize = null; });

// **CORS Configuration**
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin() // Allow requests from any origin
                .AllowAnyMethod() // Allow any HTTP method (GET, POST, etc.)
                .AllowAnyHeader(); // Allow any header.
        });

    options.AddPolicy("AllowSpecificOrigin",
        builder =>
        {
            builder.WithOrigins("http://localhost:3000") // Allow only this specific origin
                .AllowAnyMethod() // Allow any HTTP method (GET, POST, etc.)
                .AllowAnyHeader() // Allow any header
                .AllowCredentials(); // Allow credentials
        });
});


builder.Services.AddSignalR();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// **Use CORS middleware**
app.UseCors("AllowAllOrigins");
app.UseCors("AllowSpecificOrigin");

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Cache-Control", "no-store");
    }
});

app.UseRouting();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<AudioStreamHub>("/audioStreamHub");
    endpoints.MapControllers();
});
 



app.Run();