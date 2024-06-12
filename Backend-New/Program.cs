using Backend;
using Backend.Middleware;
using Backend.Models;
using Backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configure CORS
builder.Services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
{
    builder.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader();
}));

// Configure Settings (reading appsettings.json / env variables)
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("App"));
builder.Services.Configure<DiscordSettings>(builder.Configuration.GetSection("Discord"));
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("Databases"));
builder.Services.Configure<MigrationSettings>(builder.Configuration.GetSection("Migration"));

// Add Services to the container.
builder.Services.AddHttpClient<DataService>();
builder.Services.AddTransient<UploadService>();
builder.Services.AddSingleton<DiscordService>();

// Add Middleware
builder.Services.AddScoped<AuthenticationMiddleware>();

// Add DBContext
builder.Services.AddDbContextFactory<VideosMigrationContext>(options =>
{
    options.UseMySql(builder.Configuration["Databases:StandardString"], ServerVersion.Parse("10.4.20-mariadb"), providerOptions => providerOptions.EnableRetryOnFailure());
});
builder.Services.AddDbContextFactory<VideosContext>(options => {
    options.UseSqlite("Data Source=videos.db");
});
builder.Services.AddScoped(p => p.GetRequiredService<IDbContextFactory<VideosContext>>().CreateDbContext());
//builder.Services.AddScoped(p => p.GetRequiredService<IDbContextFactory<VideosMigrationContext>>().CreateDbContext());
//builder.Services.AddHostedService<MigrationService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Backend", Version = "v1" });
});

var app = builder.Build();

// Enable MyPolicy of CORS for local development
app.UseCors("MyPolicy");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Set Service and Middleware
app.Services.GetRequiredService<DiscordService>();
app.UseMiddleware<AuthenticationMiddleware>();


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
