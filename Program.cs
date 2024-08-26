using System.Text.Json.Serialization;
using Three2025.Components;
using FoundryRulesAndUnits.Units;
using BlazorComponentBus;
using Radzen;
using FoundryRulesAndUnits.Extensions;

using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.Http.Features;
using FoundryBlazor.Solutions;
using FoundryBlazor.Shape;
using FoundryBlazor.Shared;
using FoundryBlazor;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder
            // .WithOrigins(new[] { "http://localhost:8080", "http://localhost:8081" })
            //.AllowCredentials()
            .AllowAnyHeader()
            .SetIsOriginAllowed(_ => true)
            .AllowAnyOrigin()
            .AllowAnyMethod();
    });
});


// var blobKey = builder.Configuration.GetValue<string>("StorageConnectionString");
// builder.Services.AddScoped(x => new BlobServiceClient(blobKey));

builder.Services.AddCascadingAuthenticationState();



//Services setup
builder.Services.Configure<StaticFileOptions>(options =>
{
    options.ContentTypeProvider = new FileExtensionContentTypeProvider
    {
        Mappings =
        {
            [".gltf"] = "model/gltf+json",
            [".glb"] = "model/gltf-binary"
        }
    };
});

// var envVaultConfig = new VaultEnvConfig("./.env");
// builder.Services.AddSingleton<IVaultEnvConfig>(provider => envVaultConfig);

var envConfig = new EnvConfig("./.env");
builder.Services.AddSingleton<IEnvConfig>(provider => envConfig);
//builder.Services.AddScoped<IAzureBlobService, AzureBlobService>();
//builder.Services.AddScoped<IRestMentorService, RestMentorService>();
// builder.Services.AddScoped<IFileService, FileService>();

//Mentor Services
builder.Services.AddScoped<ComponentBus>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<IToast, Toast>();

builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<IPopupDialog, PopupDialog>();

builder.Services.AddScoped<IQRCodeService, QRCodeService>();
builder.Services.AddScoped<ICommand, CommandService>();
builder.Services.AddScoped<IPanZoomService, PanZoomService>();
builder.Services.AddScoped<IDrawing, FoDrawing2D>();
builder.Services.AddScoped<IArena, FoArena3D>();

builder.Services.AddScoped<IPageManagement, PageManagementService>();
builder.Services.AddScoped<IHitTestService, HitTestService>();
builder.Services.AddScoped<ISelectionService, SelectionService>();

builder.Services.AddScoped<IStageManagement, StageManagementService>();
builder.Services.AddScoped<IFoundryService, FoundryService>();

builder.Services.AddScoped<IWorkspace, FoWorkspace>();
builder.Services.AddScoped<IWorkbook, FoWorkbook>();


builder.Services.AddScoped<IUnitSystem, UnitSystem>();

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddDirectoryBrowser();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    var ser = options.JsonSerializerOptions;
    ser.IgnoreReadOnlyFields = true;
    ser.IncludeFields = true;
    ser.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    ser.WriteIndented = true;
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddRadzenComponents();

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddDirectoryBrowser();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    var ser = options.JsonSerializerOptions;
    ser.IgnoreReadOnlyFields = true;
    ser.IncludeFields = true;
    ser.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    ser.WriteIndented = true;
});

builder.Services.AddHttpClient();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// http://binaryintellect.net/articles/612cf2d1-5b3d-40eb-a5ff-924005955a62.aspx
// This code configures the FormOptions and sets the MultipartBodyLengthLimit 
// property to 200 MB.
builder.Services.Configure<FormOptions>(x =>
{
    x.MultipartBodyLengthLimit = 209715200;
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

var serviceScope = ((IApplicationBuilder)app).ApplicationServices
    .GetRequiredService<IServiceScopeFactory>()
    .CreateScope();

var unitsystem = serviceScope.ServiceProvider.GetService<IUnitSystem>();
unitsystem?.Apply(UnitSystemType.MKS);


var root = Directory.GetCurrentDirectory();



app.Run();
