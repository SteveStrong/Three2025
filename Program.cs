using System.Text.Json.Serialization;
using Three2025.Components;
using FoundryRulesAndUnits.Units;
using Radzen;
using FoundryRulesAndUnits.Extensions;
using FoundryMentorModeler;

using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.Components.Server.Circuits;

using Microsoft.Extensions.FileProviders;
using Three2025.Apprentice;
using Three2025.Services.Visualization;
using FoundryWorldsAndDrawings;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configure Blazor Server circuit options to support multiple tabs
builder.Services.AddServerSideBlazor(options =>
    {
        // CRITICAL: Prevent hub from timing out inactive tabs
        options.DetailedErrors = true;
        //options.MaximumReceiveMessageSize = 32 * 1024 * 1024; // 32MB for large geometry batches
    })
    .AddCircuitOptions(options =>
    {
        options.DetailedErrors = true; // Enable detailed error messages
        options.DisconnectedCircuitMaxRetained = 100;
        options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(3);
        options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(1);
        options.MaxBufferedUnacknowledgedRenderBatches = 20;
        // Note: MaximumReceiveMessageSize moved to HubOptions below
    })
    .AddHubOptions(options =>
    {
        // CRITICAL: Keep both tabs alive with generous timeouts
        options.ClientTimeoutInterval = TimeSpan.FromMinutes(5); // How long server waits for client pings
        options.HandshakeTimeout = TimeSpan.FromSeconds(30);
        options.KeepAliveInterval = TimeSpan.FromSeconds(15); // Server pings client every 15s
        options.MaximumReceiveMessageSize = 32 * 1024 * 1024; // 32MB
        options.StreamBufferCapacity = 10;
    });

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
// Enable circuit monitoring (logs when tabs connect/disconnect)
builder.Services.AddScoped<CircuitHandler, CustomCircuitHandler>();

var provider = new FileExtensionContentTypeProvider();
builder.Services.Configure<StaticFileOptions>(options =>
{
    foreach (var item in FileExtensionHelpers.MIMETypeData())
        if ( !provider.Mappings.ContainsKey(item.Key))
            provider.Mappings.Add(item.Key,item.Value);
    
    options.ContentTypeProvider = provider;
});

var envConfig = new EnvConfig("./.env");
builder.Services.AddFoundryWorldsAndDrawingsServices(envConfig);
builder.Services.AddFoundryMentorModelerServices();

builder.Services.AddScoped<IRackTech, RackTech>();
builder.Services.AddScoped<ICageTech, CageTech>();
builder.Services.AddScoped<IClockTech, ClockTech>();
builder.Services.AddScoped<ICuckooClockTech, CuckooClockTech>();
builder.Services.AddScoped<ITrisocTech, TrisocTech>();
builder.Services.AddScoped<IGeometryTech, GeometryTech>();
builder.Services.AddScoped<IModelTech, ModelTech>();

// Register tool provider for agent system (Phase 0) - MUST BE SCOPED to access scoped technicians
builder.Services.AddScoped<Three2025.Services.Agents.ITechnicianToolProvider, Three2025.Services.Agents.TechnicianToolProvider>();

// Register multi-provider chat service
builder.Services.AddScoped<Three2025.Services.Chat.IMultiProviderChatService, Three2025.Services.Chat.MultiProviderChatService>();

// Register multi-agent orchestration services (Phase 2)
builder.Services.AddScoped<Three2025.Services.Chat.IChatOrchestrator, Three2025.Services.Chat.ChatOrchestrator>();
builder.Services.AddScoped<Three2025.Services.Chat.IAgentFactory, Three2025.Services.Chat.AgentFactory>();

// Register geometry visualization service
builder.Services.AddScoped<IGeometryVisualizationService, GeometryVisualizationService>();

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

// ═══════════════════════════════════════════════════════════════
// GITHUB MODELS HEALTH CHECK
// ═══════════════════════════════════════════════════════════════
var logger = app.Services.GetRequiredService<ILogger<Program>>();
var config = app.Services.GetRequiredService<IConfiguration>();

logger.LogInformation("═══════════════════════════════════════════════════════════════");
logger.LogInformation("🏥 Running GitHub Models Health Check...");

var (isHealthy, healthMessage) = await Three2025.Services.Chat.GitHubModelsHealthCheck.CheckHealthAsync(config, logger);

if (isHealthy)
{
    logger.LogInformation($"{healthMessage}");
}
else
{
    logger.LogWarning("═══════════════════════════════════════════════════════════════");
    logger.LogWarning($"⚠️  WARNING: {healthMessage}");
    logger.LogWarning("⚠️  AI chat features may not work properly.");
    logger.LogWarning("⚠️  Consider switching to a different provider or waiting.");
    logger.LogWarning("═══════════════════════════════════════════════════════════════");
}

logger.LogInformation("═══════════════════════════════════════════════════════════════");
// ═══════════════════════════════════════════════════════════════

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

// Run matrix rotation tests at startup
//Three2025.Matrix3RotationSimpleTest.Run();

//this pull the 3d model files and others to the storage folder
//envConfig.RefreshStaticFiles();

app.Run();
