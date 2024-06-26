using Backend;
using Backend.Middleware;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configure CORS
// builder.Services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
// {
//     builder.AllowAnyOrigin()
//            .AllowAnyMethod()
//            .AllowAnyHeader();
// }));

// Configure Settings (reading appsettings.json / env variables)
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("App"));
builder.Services.Configure<DiscordSettings>(builder.Configuration.GetSection("Discord"));
builder.Services.Configure<FilesSettings>(builder.Configuration.GetSection("Files"));

// Add Services to the container.
builder.Services.AddHttpClient<DataService>();
builder.Services.AddSingleton<DiscordService>();

// Add Middleware
builder.Services.AddScoped<AuthenticationMiddleware>();

// Add DBContext
builder.Services.AddDbContextFactory<VideosContext>(options => {
    options.UseSqlite("Data Source=" + Environment.GetEnvironmentVariable("Files__Path") + "\\videos.db");
});

Console.WriteLine("DB should be here: " + Environment.GetEnvironmentVariable("Files__Path") + "\\videos.db");

builder.Services.AddScoped(p => p.GetRequiredService<IDbContextFactory<VideosContext>>().CreateDbContext());

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Backend", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme. \r\n\r\n 
                      Enter 'Bearer' [space] and then your token in the text input below.
                      \r\n\r\nExample: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
});

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = long.MaxValue;
});

builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = long.MaxValue;
});

var app = builder.Build();

// Enable MyPolicy of CORS for local development
// app.UseCors("MyPolicy");

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
