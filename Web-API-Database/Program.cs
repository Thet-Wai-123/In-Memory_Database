using System.Data;
using Web_API_Database;
using Web_API_Database.DataTypes;
using IDataColumn = Web_API_Database.DataTypes.IDataColumn;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
