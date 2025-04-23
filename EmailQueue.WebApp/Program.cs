using EmailQueue.WebApp.Platform;
using EmailQueue.WebApp.Platform.Authentication;
using EmailQueue.WebApp.Platform.Logging;
using EmailQueue.WebApp.Services;
using Microsoft.AspNetCore.DataProtection;
using Mindscape.Raygun4Net.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Populate application settings.
builder.BindSettings();

// Configure authentication.
builder.AddAuthenticationServices();

// Persist data protection keys.
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(Directory.CreateDirectory(AppSettings.DataProtectionKeysFolder));

// Configure the EmailQueue API.
builder.Services.AddHttpClient<EmailQueueApiService>();
builder.Services.AddScoped<EmailQueueApiService>();

// Configure UI services.
builder.Services.AddRazorPages();

// Configure HSTS.
// TODO: Change to `FromDays` in production.
builder.Services.AddHsts(opts => opts.MaxAge = TimeSpan.FromSeconds(360));

// Configure application crash monitoring.
builder.AddErrorLogging();

var app = builder.Build();

// Configure error handling.
if (app.Environment.IsDevelopment()) app.UseDeveloperExceptionPage();
else app.UseExceptionHandler("/Error");

if (!string.IsNullOrEmpty(AppSettings.RaygunSettings.ApiKey)) app.UseRaygun();

// Configure the application pipeline.
app.UseHsts();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
    .WithStaticAssets()
    .RequireAuthorization();

await app.RunAsync();
