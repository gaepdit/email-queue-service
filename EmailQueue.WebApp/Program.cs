using EmailQueue.WebApp.Platform;
using EmailQueue.WebApp.Platform.DataProtection;
using EmailQueue.WebApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Populate application settings.
builder.BindSettings();

// Persist data protection keys.
await builder.AddDataProtectionServices();

// Configure the EmailQueue API.
builder.Services.AddHttpClient<EmailQueueApiService>();
builder.Services.AddScoped<EmailQueueApiService>();

// Configure UI services.
builder.Services.AddRazorPages();

// Configure HSTS.
// TODO: Change to `FromDays` in production.
builder.Services.AddHsts(opts => opts.MaxAge = TimeSpan.FromSeconds(360));

var app = builder.Build();

// Configure error handling.
if (app.Environment.IsDevelopment()) app.UseDeveloperExceptionPage();
else app.UseExceptionHandler("/Error");

// Configure the application pipeline.
app.UseHsts();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

await app.RunAsync();
