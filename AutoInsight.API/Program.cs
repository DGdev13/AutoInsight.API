using AutoInsight.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Register HttpClient and VehicleService
builder.Services.AddHttpClient<IVehicleService, VehicleService>();

// Add Swagger/OpenAPI with metadata
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "AutoInsight VIN Decoder API",
        Version = "v1",
        Description = "API to decode vehicle VIN numbers and estimate pricing. Built for use on RapidAPI.",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Diego Garcia",
            Email = "your-email@example.com"
        }
    });
});

// Enable CORS (allow all origins, methods, headers)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Use Swagger and Swagger UI in all environments (optional: restrict to Development)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.DocumentTitle = "AutoInsight VIN Decoder API Docs";
});

// Use HTTPS redirection
app.UseHttpsRedirection();

app.UseAuthorization();
app.UseCors();

app.MapControllers();

// Root endpoint: API info
app.MapGet("/", () => Results.Ok(new
{
    name = "AutoInsight VIN Decoder API",
    version = "v1",
    status = "online",
    docs = "/swagger"
}));

// Health check endpoint
app.MapGet("/health", () => Results.Ok("Healthy"));

app.Run();
