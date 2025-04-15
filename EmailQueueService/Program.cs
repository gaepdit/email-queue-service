using EmailQueueService.Data;
using EmailQueueService.Models;
using EmailQueueService.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    const string securityDefinitionName = "ApiKeyHeader";
    options.AddSecurityDefinition(securityDefinitionName, new OpenApiSecurityScheme()
    {
        Name = ApiKeyAuthenticationHandler.ApiKeyHeaderName,
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
    });
    var openApiSecurityScheme = new OpenApiSecurityScheme
        { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = securityDefinitionName } };
    options.AddSecurityRequirement(new OpenApiSecurityRequirement { { openApiSecurityScheme, [] }, });
});

// Configure API Key Authentication
builder.Services.AddAuthentication(nameof(SecuritySchemeType.ApiKey))
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(nameof(SecuritySchemeType.ApiKey), null);

// Add database context
builder.Services.AddDbContext<EmailQueueDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure settings
builder.Services.Configure<EmailQueueSettings>(
    builder.Configuration.GetSection(EmailQueueSettings.SectionName));

// Register our services
builder.Services.AddSingleton<IQueueService, QueueService>();
builder.Services.AddHostedService<EmailProcessorService>();

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<EmailQueueDbContext>();
    await context.Database.EnsureCreatedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();
