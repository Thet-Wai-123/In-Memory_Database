using In_Memory_Database;
using In_Memory_Database.Classes;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
Startup startup = new();
startup.ConfigureServices(builder.Services);

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
