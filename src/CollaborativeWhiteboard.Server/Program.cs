using CollaborativeWhiteboard.Server.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);

var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddSignalR();
builder.Services.AddSingleton<ConcurrentDictionary<string, string>>(); // userId → connectionId map
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", b =>
    {
        b.AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();

        if (builder.Environment.IsDevelopment() || allowedOrigins.Length == 0)
            b.SetIsOriginAllowed(_ => true);
        else
            b.WithOrigins(allowedOrigins);
    });
});

var app = builder.Build();

app.UseCors("CorsPolicy");

app.UseRouting();

app.MapHub<DrawingHub>("/drawingHub");

app.Run();