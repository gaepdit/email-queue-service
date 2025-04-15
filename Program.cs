using EmailQueueService.Services;
using EmailQueueService.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure settings
builder.Services.Configure<EmailQueueSettings>(
    builder.Configuration.GetSection(EmailQueueSettings.SectionName));

// Register our services
builder.Services.AddSingleton<IQueueService, QueueService>();
builder.Services.AddHostedService<DataProcessorService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();
