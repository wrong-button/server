using ExitPath.Server.Config;
using ExitPath.Server.Multiplayer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace ExitPath.Server
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
            services.AddOptions<AuthConfig>()
                .Bind(Configuration.GetSection("Multiplayer:Auth"))
                .ValidateDataAnnotations();
            services.AddOptions<RealmConfig>()
                .Bind(Configuration.GetSection("Multiplayer"))
                .ValidateDataAnnotations();
            services.AddOptions<HTTPConfig>()
                .Bind(Configuration)
                .ValidateDataAnnotations();

            services.AddSingleton<AuthTokenService>();
            services.AddSingleton<Realm>();
            services.AddHostedService<RealmRunner>();

            services.AddCors();
            services.AddSingleton<IPostConfigureOptions<CorsOptions>, CORSPostConfigurer>();

            services.AddAuthentication()
                .AddJwtBearer("Multiplayer", _ => { });
            services.AddSingleton<IPostConfigureOptions<JwtBearerOptions>, JwtBearerPostConfigurer>();

            services.AddControllers();
            services.AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHttpsRedirection();
            }


            app.UseRouting();
            app.UseCors();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<MultiplayerHub>("/api/multiplayer/hub");
            });
        }
    }

    public class CORSPostConfigurer : IPostConfigureOptions<CorsOptions>
    {
        private readonly HTTPConfig config;

        public CORSPostConfigurer(IOptions<HTTPConfig> config)
        {
            this.config = config.Value;
        }

        public void PostConfigure(string name, CorsOptions options)
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.WithOrigins(this.config.AllowedOrigins);
                builder.AllowAnyHeader();
                builder.AllowCredentials();
            });
        }
    }

    public class JwtBearerPostConfigurer : IPostConfigureOptions<JwtBearerOptions>
    {
        private readonly AuthConfig config;

        public JwtBearerPostConfigurer(IOptions<AuthConfig> config)
        {
            this.config = config.Value;
        }

        public void PostConfigure(string name, JwtBearerOptions options)
        {
            if (name != "Multiplayer")
            {
                return;
            }

            var credentials = this.config.CreateCredentials();
            options.TokenValidationParameters.ValidateIssuerSigningKey = true;
            options.TokenValidationParameters.IssuerSigningKey = credentials.Key;
            options.TokenValidationParameters.ValidAlgorithms = new[] { credentials.Algorithm };
            options.TokenValidationParameters.ValidAudience = this.config.Authority;
            options.TokenValidationParameters.ValidIssuer = this.config.Authority;
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                }
            };
        }
    }
}
