using Backend.Middleware;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace Backend
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Localhost developement
            //services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
            //{
            //    builder.AllowAnyOrigin()
            //           .AllowAnyMethod()
            //           .AllowAnyHeader();
            //}));

            services.Configure<GoogleSettings>(Configuration.GetSection("Google"));
            services.Configure<AppSettings>(Configuration.GetSection("App"));
            services.Configure<DiscordSettings>(Configuration.GetSection("Discord"));
            services.Configure<DatabaseSettings>(Configuration.GetSection("Databases"));
            services.Configure<MigrationSettings>(Configuration.GetSection("Migration"));

            services.AddHttpClient<DataService>();
            services.AddTransient<UploadService>();
            services.AddSingleton<DiscordService>();
            services.AddScoped<AuthenticationMiddleware>();
            services.AddDbContextFactory<VideosContext>(options => {
                options.UseMySql(Configuration["Databases:StandardString"], ServerVersion.Parse("10.4.20-mariadb"), providerOptions => providerOptions.EnableRetryOnFailure());
            });
            services.AddScoped(p => p.GetRequiredService<IDbContextFactory<VideosContext>>().CreateDbContext());
            services.AddScoped<GoogleService>();

            if (Configuration["Migration:MigrationString"] != "" && Configuration["Migration:ClipsPath"] != "")
            {
                services.AddDbContextFactory<VideosMigrationContext>(options =>
                {
                    options.UseMySql(Configuration["Migration:MigrationString"], ServerVersion.Parse("10.4.20-mariadb"));
                });
                services.AddScoped(p => p.GetRequiredService<IDbContextFactory<VideosMigrationContext>>().CreateDbContext());
                services.AddHostedService<MigrationService>();
            }

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Backend", Version = "v1" });
            });

            // To allow larger files than 30MB:
            services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = int.MaxValue;
            });

            services.Configure<FormOptions>(options =>
            {
                options.ValueLengthLimit = int.MaxValue;
                options.MultipartBodyLengthLimit = int.MaxValue;
                options.MultipartHeadersLengthLimit = int.MaxValue;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, VideosContext context)
        {
            context.Database.EnsureCreated();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Backend v1"));
            }

            // Localhost developement
            //app.UseCors("MyPolicy");

            app.ApplicationServices.GetRequiredService<DiscordService>();
            app.UseMiddleware<AuthenticationMiddleware>();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
