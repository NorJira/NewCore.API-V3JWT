﻿using System;
using JWTClassLib;
using JWTClassLib.Helpers;
using JWTClassLib.Middleware;
using JWTClassLib.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NewCore.Data.Context;
using NewCore.Services.CustomerServices;
using NewCore.Services.Interfaces;
using NewCore.Services.PolCvgServices;
using NewCore.Services.PolicyServices;

namespace NewCore.API
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
            // Add Data Layer
            //services.AddDbContext<NewCoreDataContext>(options =>
            //{
            //    options.UseSqlServer(Configuration.GetConnectionString("TestDBConnection"));
            //});
            services.AddEntityFrameworkSqlServer();

            // configure strongly typed settings object
            services.Configure<JWTSettings>(Configuration.GetSection("JWTSettings"));

            // Add dbcontext
            services.AddDbContext<NewCoreDataContext>(options =>
                {
                    options.UseSqlServer(Configuration.GetConnectionString("TestDBConnection"));
                });
            services.AddDbContext<JWTDataContext>();

            // Dependency Injection - Services
            services.AddScoped<ICustomerServices, CustomerServices>()
                .AddScoped<IPolicyServices, PolicyServices>()
                .AddScoped<IPolCvgServices, PolCvgServices>();

            // configure DI for JWT application & email //services
            services.AddScoped<IAccountService, AccountService>()
                .AddScoped<IEmailService, EmailService>();

            //.AddTransient<>; 

            //services.AddSingleton<CusDtoConversion>();

            // Add Business Layer
            //services.AddTransient<ICustomerServices, CustomerServices>();
            //services.AddTransient<IPolicyServices, PolicyServices>();

            // services.AddControllers();
            services.AddControllers(options =>
               options.SuppressAsyncSuffixInActionNames = true
            ).AddJsonOptions(options =>
               options.JsonSerializerOptions.WriteIndented = true
            );

            // Add AutoMapper for JWT
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            // Add HTTP Client
            services.AddHttpClient();

            // Add CORS
            services.AddCors(options => {
                options.AddDefaultPolicy(
                    builder => builder.AllowAnyOrigin().AllowAnyMethod()
                    // with specify origin "http:.... must not end with '/'
                    // builder => builder.WithOrigins(
                    //     "http://localhost:5001",
                    //     "http://localhost:5100"
                    //     )
                    //     .WithMethods("GET","PUT","POST","DELETE")
                );
                // 
                // options.AddPolicy("mypolicy",
                //     builder => builder.WithOrigins("http://localhost:5001")                    
                // );

            });

            // Add health check
            //services.AddHealthChecks();
                
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            // NOR - middleware CORS must be after userouting() and before useauthorization()
            //app.UseCors();  // use default
            app.UseCors(x => x
                .SetIsOriginAllowed(origin => true)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());

            //app.UseCors(x =>x
            //    .AllowAnyOrigin()
            //    .WithMethods("httppost")
            //    .AllowAnyHeader()
            //);
            //app.UseCors("mypolicy");  // use mypolicy

            // global error handler
            //app.UseMiddleware<ErrorHandlerMiddleware>();

            // custom jwt auth middleware
            app.UseMiddleware<JwtMiddleware>();

            //app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
