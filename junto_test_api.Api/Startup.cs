using AutoMapper;
using junto_test_api.Domain.Mapping;
using junto_test_api.Domain.Service;
using junto_test_api.Entity.Context;
using junto_test_api.Entity.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Text;

namespace junto_test_api.Api
{
    public class Startup
    {

        public static IConfiguration Configuration { get; set; }

        public IWebHostEnvironment HostingEnvironment { get; private set; }

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            HostingEnvironment = env;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            Log.Information("Startup::ConfigureServices");

            try
            {
                services.AddControllers(
                opt =>
                {
                    //Custom filters can be added here 
                    //opt.Filters.Add(typeof(CustomFilterAttribute));
                    //opt.Filters.Add(new ProducesAttribute("application/json"));
                }
                ).SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

                #region "API versioning"
                //API versioning service
                services.AddApiVersioning(
                    o =>
                    {
                        //o.Conventions.Controller<UserController>().HasApiVersion(1, 0);
                        o.AssumeDefaultVersionWhenUnspecified = true;
                        o.ReportApiVersions = true;
                        o.DefaultApiVersion = new ApiVersion(1, 0);
                        o.ApiVersionReader = new UrlSegmentApiVersionReader();
                    }
                    );

                // format code as "'v'major[.minor][-status]"
                services.AddVersionedApiExplorer(
                options =>
                {
                    options.GroupNameFormat = "'v'VVV";
                    //versioning by url segment
                    options.SubstituteApiVersionInUrl = true;
                });
                #endregion

                //db service
                if (Configuration["ConnectionStrings:UseInMemoryDatabase"] == "True")
                    services.AddDbContext<DBContext>(opt => opt.UseInMemoryDatabase("TestDB-" + Guid.NewGuid().ToString()));
                else
                    services.AddDbContext<DBContext>(options => options.UseSqlServer(Configuration["ConnectionStrings:Database"]));

                #region "CORS"
                // include support for CORS
                // More often than not, we will want to specify that our API accepts requests coming from other origins (other domains). When issuing AJAX requests, browsers make preflights to check if a server accepts requests from the domain hosting the web app. If the response for these preflights don't contain at least the Access-Control-Allow-Origin header specifying that accepts requests from the original domain, browsers won't proceed with the real requests (to improve security).
                services.AddCors(options =>
                {
                    options.AddPolicy("CorsPolicy-public",
                        builder => builder.AllowAnyOrigin()   //WithOrigins and define a specific origin to be allowed (e.g. https://mydomain.com)
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                    .Build());
                });
                #endregion

                //mvc service
                services.AddMvc(option => option.EnableEndpointRouting = false)
                     .AddNewtonsoftJson();

                #region "DI code"
                //general unitofwork injections
                services.AddTransient<IUnitOfWork, UnitOfWork>();

                //services injections
                services.AddTransient(typeof(IAccountService<,>), typeof(AccountService<,>));
                services.AddTransient(typeof(IUserService<,>), typeof(UserService<,>));
                services.AddTransient(typeof(ITokenService<,>), typeof(TokenService<,>));
                //...add other services
                //
                services.AddTransient(typeof(IService<,>), typeof(GenericService<,>));
                #endregion

                //data mapper services configuration
                services.AddAutoMapper(typeof(MappingProfile));

                #region "Swagger API"
                //Swagger API documentation
                services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "junto_test_api API", Version = "v1" });
                    c.SwaggerDoc("v2", new OpenApiInfo { Title = "junto_test_api API", Version = "v2" });

                    //In Test project find attached swagger.auth.pdf file with instructions how to run Swagger authentication 
                    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                    {
                        Description = "Authorization header using the Bearer scheme",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey
                    });

                    c.AddSecurityRequirement(new OpenApiSecurityRequirement{
                        {
                            new OpenApiSecurityScheme{
                                Reference = new OpenApiReference{
                                    Id = "Bearer", //The name of the previously defined security scheme.
                                	Type = ReferenceType.SecurityScheme
                                }
                            },new List<string>()
                        }
                    });

                    //c.DocumentFilter<api.infrastructure.filters.SwaggerSecurityRequirementsDocumentFilter>();
                });
                #endregion
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            Log.Information("Startup::Configure");

            try
            {
                if (env.EnvironmentName == "Development")
                    app.UseDeveloperExceptionPage();
                else
                    app.UseMiddleware<ExceptionHandler>();

                app.UseCors("CorsPolicy-public");  //apply to every request
                app.UseAuthentication(); //needs to be up in the pipeline, before MVC
                app.UseAuthorization();

                app.UseMvc();

                //Swagger API documentation
                app.UseSwagger();

                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "junto_test_api API V1");
                    c.SwaggerEndpoint("/swagger/v2/swagger.json", "junto_test_api API V2");
                    c.DisplayOperationId();
                    c.DisplayRequestDuration();
                });

                //migrations and seeds from json files
                using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    if (Configuration["ConnectionStrings:UseInMemoryDatabase"] == "False" && !serviceScope.ServiceProvider.GetService<DBContext>().AllMigrationsApplied())
                    {
                        if (Configuration["ConnectionStrings:UseMigrationService"] == "True")
                            serviceScope.ServiceProvider.GetService<DBContext>().Database.Migrate();
                    }
                    //it will seed tables on aservice run from json files if tables empty
                    if (Configuration["ConnectionStrings:UseSeedService"] == "True")
                        serviceScope.ServiceProvider.GetService<DBContext>().EnsureSeeded();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }

        }
    }
}


namespace api.infrastructure.filters
{
    public class SwaggerSecurityRequirementsDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument document, DocumentFilterContext context)
        {
            document.SecurityRequirements = new List<OpenApiSecurityRequirement>
            {
                new OpenApiSecurityRequirement{
                    {
                        new OpenApiSecurityScheme{
                            Reference = new OpenApiReference{
                                Id = "Bearer", //The name of the previously defined security scheme.
                                Type = ReferenceType.SecurityScheme
                            }
                        },new List<string>()
                    }
                },
                new OpenApiSecurityRequirement{
                    {
                        new OpenApiSecurityScheme{
                            Reference = new OpenApiReference{
                                Id = "Basic", //The name of the previously defined security scheme.
                                Type = ReferenceType.SecurityScheme
                            }
                        },new List<string>()
                    }
                }
             };

        }
    }
}







