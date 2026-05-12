// Program.cs - Entry point and configuration for the ASP.NET Core application.
// We set up dependency injection, middleware, and routing here before calling app.Run().

using AiMaturityApp.Model.Repositories;
using AiMaturityApp.Services;

var builder = WebApplication.CreateBuilder(args);

// DATABASE_URL support - Railway (and most cloud platforms) inject the
// connection string as a single DATABASE_URL environment variable in the
// format: postgresql://user:password@host:port/database
// If that variable is present we parse it into an Npgsql connection string
// and inject it so BaseRepository picks it up via IConfiguration.
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(databaseUrl))
{
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':');
    var npgsqlConn = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};" +
                     $"Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
    builder.Configuration["ConnectionStrings:AppProgDb"] = npgsqlConn;
}

// AddControllers - registers all [ApiController] classes so ASP.NET Core
// can route incoming HTTP requests to the right method.
builder.Services.AddControllers();

// AddScoped - one instance per HTTP request.
// We use scoped lifetime for repositories and services because each
// request opens its own database connection.
builder.Services.AddScoped<AiMaturityAssessmentRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<ScoringService>();

// Swagger - generates interactive API docs at /swagger during development.
// Useful for manually testing endpoints without the frontend.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS - allows the frontend HTML file (served from file://) to call
// the API at http://localhost:5295 without the browser blocking it.
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// UseCors must come before MapControllers so headers are added to every response.
app.UseCors();

app.MapControllers();

// Health check endpoint - Railway pings this to confirm the app started correctly.
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();
