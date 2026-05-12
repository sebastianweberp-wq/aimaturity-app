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

// Auto-initialise the database on every cold start.
// CREATE TABLE IF NOT EXISTS means this is safe to run repeatedly - it skips
// creation if the tables already exist and never deletes existing data.
// This removes the need for a manual pgAdmin seeding step when deploying to
// Railway or any other cloud platform that provisions a fresh PostgreSQL instance.
try
{
    var connStr = app.Configuration.GetConnectionString("AppProgDb");
    if (!string.IsNullOrEmpty(connStr))
    {
        using var conn = new Npgsql.NpgsqlConnection(connStr);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS public.appuser (
                id       SERIAL PRIMARY KEY,
                username TEXT NOT NULL
            );
            CREATE UNIQUE INDEX IF NOT EXISTS appuser_username_idx ON public.appuser (username);

            CREATE TABLE IF NOT EXISTS public.aimaturityassessment (
                id                  SERIAL PRIMARY KEY,
                username            TEXT    NOT NULL DEFAULT '',
                companyname         TEXT    NOT NULL,
                industry            TEXT    NOT NULL,
                companysize         TEXT    NOT NULL,
                digitalmaturity     INTEGER NOT NULL CHECK (digitalmaturity   BETWEEN 1 AND 5),
                datareadiness       INTEGER NOT NULL CHECK (datareadiness     BETWEEN 1 AND 5),
                currentaiusage      INTEGER NOT NULL CHECK (currentaiusage    BETWEEN 1 AND 5),
                processautomation   INTEGER NOT NULL CHECK (processautomation BETWEEN 1 AND 5),
                employeeskills      INTEGER NOT NULL CHECK (employeeskills    BETWEEN 1 AND 5),
                managementsupport   INTEGER NOT NULL CHECK (managementsupport BETWEEN 1 AND 5),
                budgetreadiness     INTEGER NOT NULL CHECK (budgetreadiness   BETWEEN 1 AND 5),
                totalscore          INTEGER NOT NULL CHECK (totalscore        BETWEEN 0 AND 100),
                maturitylevel       TEXT    NOT NULL
            );

            INSERT INTO public.appuser (username)
            SELECT u FROM (VALUES ('Sebastian'),('Maria'),('Jonas'),('Sofie'),('Andreas')) AS t(u)
            WHERE NOT EXISTS (SELECT 1 FROM public.appuser LIMIT 1);

            INSERT INTO public.aimaturityassessment
                (username, companyname, industry, companysize,
                 digitalmaturity, datareadiness, currentaiusage,
                 processautomation, employeeskills, managementsupport,
                 budgetreadiness, totalscore, maturitylevel)
            SELECT * FROM (VALUES
                ('Sebastian','Copenhagen Autoservice','Automotive & repair','Small (10-49 employees)',2,2,1,1,2,3,2,30,'Low AI Maturity'),
                ('Maria','Nordic Bakery ApS','Retail & e-commerce','Micro (2-9 employees)',1,1,1,1,1,2,1,17,'Low AI Maturity'),
                ('Jonas','Buildex Construction','Construction & trades','Small (10-49 employees)',2,3,2,2,2,2,2,34,'Low AI Maturity'),
                ('Sofie','Bright Legal','Professional services (legal, accounting, consulting)','Small (10-49 employees)',3,4,3,3,3,4,3,64,'Medium AI Maturity'),
                ('Andreas','HealthFirst Clinic','Healthcare & wellness clinics','Small (10-49 employees)',4,3,3,3,4,4,3,68,'Medium AI Maturity'),
                ('Sebastian','FitLife Studio','Beauty, fitness & personal care','Micro (2-9 employees)',3,3,2,3,3,3,3,54,'Medium AI Maturity'),
                ('Maria','CreativeHub','Creative & media (design, advertising, marketing)','Micro (2-9 employees)',4,4,4,3,4,4,4,78,'High AI Maturity'),
                ('Jonas','LogiFlow Logistics','Logistics, transport & courier','Medium (50-249 employees)',5,4,4,5,4,5,4,88,'High AI Maturity'),
                ('Sofie','TechEdge IT Services','IT & tech services','Small (10-49 employees)',5,5,5,4,5,5,5,97,'High AI Maturity'),
                ('Andreas','EduPro Training','Education & training','Small (10-49 employees)',3,3,3,2,3,3,2,52,'Medium AI Maturity')
            ) AS seed(username,companyname,industry,companysize,
                       digitalmaturity,datareadiness,currentaiusage,
                       processautomation,employeeskills,managementsupport,
                       budgetreadiness,totalscore,maturitylevel)
            WHERE NOT EXISTS (SELECT 1 FROM public.aimaturityassessment LIMIT 1);
        ";
        cmd.ExecuteNonQuery();
    }
}
catch (Exception ex)
{
    // Log but don't crash - the app can still start even if DB init fails.
    Console.WriteLine($"[DB Init] Warning: {ex.Message}");
}

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
