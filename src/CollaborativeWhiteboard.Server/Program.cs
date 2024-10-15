using CollaborativeWhiteboard.Server.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", b =>
    {
        b.AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
            .WithOrigins("https://your-client-url.com");
    });
});

var app = builder.Build();

app.UseCors("CorsPolicy");

app.UseRouting();

app.MapHub<DrawingHub>("/drawingHub");

app.Run();