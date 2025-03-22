using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Application.Interfaces;
using Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IAppConfiguration, ConfigurationService>();
builder.Services.AddScoped<IHardwareService, HardwareService>();

// Đăng ký dịch vụ tiến trình
builder.Services.AddScoped<IProcessService, ProcessService>();

builder.Services.AddScoped<INetworkService, NetworkService>();
builder.Services.AddScoped<LogService>();
builder.Services.AddControllers();

// Cấu hình Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();
