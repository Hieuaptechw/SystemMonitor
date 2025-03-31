using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Application.Interfaces;
using Infrastructure.Services;
using Infrastructure.Logging;
using Application.Services;
using Infrastructure.Configuration;
using Microsoft.AspNetCore.SignalR;
using API.Hubs;

var builder = WebApplication.CreateBuilder(args);
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder =>
        {
            builder.WithOrigins(allowedOrigins) 
                   .AllowAnyMethod()
                   .AllowAnyHeader()
                   .AllowCredentials();
        });
});


builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();


builder.Services.AddSingleton<IAppConfiguration, ConfigurationService>();
builder.Services.AddScoped<IHardwareService, HardwareService>();
builder.Services.AddScoped<IProcessService, ProcessService>();
builder.Services.AddScoped<INetworkService, NetworkService>();
builder.Services.AddScoped<LogService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddSignalR();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseCors("AllowSpecificOrigin"); 

app.UseRouting();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHub<HardwareHub>("/hardwareHub").RequireAuthorization();
app.MapHub<NetWorkHub>("/networkHub").RequireAuthorization();
app.MapHub<ProcessHub>("/processHub").RequireAuthorization();
app.Run();
