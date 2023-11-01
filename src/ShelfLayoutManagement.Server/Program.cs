using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;

using MongoDB.Driver;

using ShelfLayoutManagement.Common.Interceptors;
using ShelfLayoutManagement.Common.Interfaces;
using ShelfLayoutManagement.Data.Converters;
using ShelfLayoutManagement.Data.Entities;
using ShelfLayoutManagement.Data.Models;
using ShelfLayoutManagement.Data.Repositories;
using ShelfLayoutManagement.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
builder.Services.AddGrpc(options =>
{
    options.EnableDetailedErrors = true;
    options.Interceptors.Add<ExceptionInterceptor>();
    options.Interceptors.Add<TracerInterceptor>();
});

builder.Services.AddSingleton<IMongoClient>(new MongoClient(builder.Configuration.GetConnectionString("Mongo")));

var cabinetCollectionSettings = new CabinetCollectionSettings();
builder.Configuration.Bind("CabinetCollectionSettings", cabinetCollectionSettings);
builder.Services.AddSingleton<ICabinetCollectionSettings>(cabinetCollectionSettings);

var stockKeepingUnitCollectionSettings = new StockKeepingUnitCollectionSettings();
builder.Configuration.Bind("StockKeepingUnitCollectionSettings", stockKeepingUnitCollectionSettings);
builder.Services.AddSingleton<IStockKeepingUnitCollectionSettings>(stockKeepingUnitCollectionSettings);

builder.Services.AddSingleton<ICabinetRepository, CabinetRepository>();
builder.Services.AddSingleton<IStockKeepingUnitRepository, StockKeepingUnitRepository>();
builder.Services.AddTransient<IConverter<Cabinet, ShelfLayoutManagement.Common.Shelf.Cabinet>, CabinetConverter>();
builder.Services.AddTransient<IConverter<Row, ShelfLayoutManagement.Common.Shelf.Row>, RowConverter>();
builder.Services.AddTransient<IConverter<Lane, ShelfLayoutManagement.Common.Shelf.Lane>, LaneConverter>();
builder.Services.AddTransient<IConverter<Product, ShelfLayoutManagement.Common.StockKeepingUnit.Product>, ProductConverter>();

//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddMicrosoftIdentityWebApi(options =>
//        {
//            builder.Configuration.Bind("AzureAd", options);
//        },
//        options => builder.Configuration.Bind("AzureAd", options));

//builder.Services.AddAuthorization();

var app = builder.Build();

//app.UseAuthentication();
//app.UseAuthorization();

app.MapGrpcService<ShelvesService>();

// Configure the HTTP request pipeline.
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();