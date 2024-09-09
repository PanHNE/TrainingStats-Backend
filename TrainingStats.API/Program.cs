using TrainingStats.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices();
builder.Services.AddWebServerServices();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddCors();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await app.InitialiseDatabaseAsync();
}
else
{
    app.UseHsts();
}

var connectionString = Environment.GetEnvironmentVariable("FRONTEND_URL");

app.UseCors(corsBuilder =>
    corsBuilder
        .AllowAnyHeader()
        .AllowAnyMethod()
        .WithOrigins(connectionString!)
);

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapIdentityApi<ApplicationUser>();
app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
