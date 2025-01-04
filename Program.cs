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
using Three2025.Apprentice;
using Microsoft.AspNetCore.Components.Server.Circuits;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()

    .AddInteractiveServerComponents();



builder.Services.AddRadzenComponents();

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



builder.Services.AddCascadingAuthenticationState();
//builder.Services.AddSingleton<CircuitHandler, CustomCircuitHandler>();

var provider = new FileExtensionContentTypeProvider();
builder.Services.Configure<StaticFileOptions>(options =>
{
    foreach (var item in FileExtensionHelpers.MIMETypeData())
        if ( !provider.Mappings.ContainsKey(item.Key))
            provider.Mappings.Add(item.Key,item.Value);
    
    options.ContentTypeProvider = provider;
});

var envConfig = new EnvConfig("./.env");
builder.Services.AddFoundryBlazorServices(envConfig);
builder.Services.AddScoped<IApprenticeAI, ApprenticeAI>();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    var ser = options.JsonSerializerOptions;
    ser.IgnoreReadOnlyFields = true;
    ser.IncludeFields = true;
    ser.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    ser.WriteIndented = true;
});


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

app.UseStaticFiles();
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

//include the static files at wwwwroot
app.UseStaticFiles();

// Serve files from storage directory
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(storagePath),
    RequestPath = "/storage",
    ContentTypeProvider = provider,
    OnPrepareResponse = ctx =>
    {
        var headers = ctx.Context.Response.Headers;
        headers.Append("Access-Control-Allow-Origin", "*");
        headers.Append("Cache-Control", $"public, max-age=3600");
    }
});



//this pull the 3d model files and others to the storage folder
//envConfig.RefreshStaticFiles();

app.Run();
