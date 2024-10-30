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
using Microsoft.Extensions.FileProviders;

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

builder.Services.Configure<StaticFileOptions>(options =>
{
    var provider = new FileExtensionContentTypeProvider();
    foreach (var item in FileExtensionHelpers.MIMETypeData())
        if ( !provider.Mappings.ContainsKey(item.Key))
            provider.Mappings.Add(item.Key,item.Value);
    
    options.ContentTypeProvider = provider;
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
builder.Services.AddScoped<IWorldManager, WorldManager>();
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



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

var serviceScope = ((IApplicationBuilder)app).ApplicationServices
    .GetRequiredService<IServiceScopeFactory>()
    .CreateScope();

var unitsystem = serviceScope.ServiceProvider.GetService<IUnitSystem>();
unitsystem?.Apply(UnitSystemType.MKS);

var storagePath = Path.Combine(Directory.GetCurrentDirectory(), "storage");

// Enable directory browsing for the storage folder
app.UseDirectoryBrowser(new DirectoryBrowserOptions
{
    FileProvider = new PhysicalFileProvider(storagePath),
    RequestPath = "/storage"
});

// Serve files from storage directory
app.UseStaticFiles(new StaticFileOptions
{
    ServeUnknownFileTypes = true,
    FileProvider = new PhysicalFileProvider(storagePath),
    RequestPath = "/storage"
});

//app.UseStaticFiles();

var root = Directory.GetCurrentDirectory();



app.Run();
