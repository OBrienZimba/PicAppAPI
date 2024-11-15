using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PicAppAPI.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<PicAppAPIContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PicAppAPIContext") ?? throw new InvalidOperationException("Connection string 'PicAppAPIContext' not found.")));

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader()
               .WithExposedHeaders("Accept", "Connection");
    });
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Kestrel to listen on the specific IP address and port
builder.WebHost.ConfigureKestrel(options =>
{
    // Listen on localhost and the specified IP for consistency on port 7137
    options.Listen(IPAddress.Parse("192.168.148.82"), 7137, listenOptions => listenOptions.UseHttps()); // This is HTTPS for your IP
    options.Listen(IPAddress.Loopback, 7137, listenOptions => listenOptions.UseHttps()); // This is HTTPS for localhost
});

// Configure HttpClient to bypass SSL verification
var handler = new HttpClientHandler();
handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

// Register HttpClient with custom handler
builder.Services.AddHttpClient("NoSSL", client =>
{
    client.BaseAddress = new Uri("https://192.168.148.82:7137");
})
.ConfigurePrimaryHttpMessageHandler(() => handler);


var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("AllowAll");
}

app.UseStaticFiles();

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
