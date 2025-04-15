using EmailQueueService.Services;
using EmailQueueService.Models;
using EmailQueueService.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add database context
builder.Services.AddDbContext<EmailQueueDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure settings
builder.Services.Configure<EmailQueueSettings>(
    builder.Configuration.GetSection(EmailQueueSettings.SectionName));

// Register our services
builder.Services.AddSingleton<IQueueService, QueueService>();
builder.Services.AddHostedService<DataProcessorService>();

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
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();
