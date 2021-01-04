using GoofyAlgoTrader.Configurations;
using GoofyAlgoTrader.WebApi.Filter;
using GoofyAlgoTrader.WebApi.Models.Config;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoofyAlgoTrader.WebApi
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
            services.Configure<JWTConfig>(Configuration.GetSection("JWTConfig"));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = TokenValidatedFilter.OnTokenValidated
                };
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = Configuration.GetValue<string>("JWTConfig:Issuer"),
                    ValidAudience = Configuration.GetValue<string>("JWTConfig:Audience"),
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.GetValue<string>("JWTConfig:IssuerSigningKey")))
                };
            });

            services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new DateTimeConverter());
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var error = context.ModelState.GetValidationSummary();
                    return new JsonResult(BaseResponse<List<ModelState>>.GetBaseResponse(ResponseStatusType.ParameterError, error));
                };
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "GoofyAlgoTrader.WebApi", Version = "v1" });
            });

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    builder =>
                    {
                        builder.AllowAnyOrigin().AllowAnyHeader();
                    });
            });

            services.UseGoofyAlgoTrader();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "GoofyAlgoTrader.WebApi v1"));
            }
            app.UseAuthentication();

            app.UseCors("AllowAllOrigins");
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
