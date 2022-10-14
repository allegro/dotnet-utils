using System;
using Allegro.Extensions.AspNetCore.Demo.Controllers;
using Allegro.Extensions.AspNetCore.ErrorHandling;
using Allegro.Extensions.AspNetCore.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Allegro.Extensions.AspNetCore.Demo
{
    public class Startup
    {
        private readonly IWebHostEnvironment _env;

        public Startup(IWebHostEnvironment env)
        {
            _env = env;
        }

        public void Configure(IApplicationBuilder app)
        {
            app
                .UseFluentErrorHandlingMiddleware()
                .UseRouting()
                .UseSwagger()
                .UseSwaggerUI()
                .UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddFluentErrorHandlingMiddleware(
                    logError: error => Console.WriteLine($"Error: {error.Message}"),
                    logWarning: warning => Console.WriteLine($"Warning: {warning.Message}"),
                    builder => builder.WithCustomAllegroErrorHandling()
                    )
                .AddControllers()
                .AddSkipOnProd(_env)
                .AddFluentModelStateValidationHandling(builder => builder.WithCustomAllegroErrorHandling());

            services.AddSwaggerGen();
        }
    }
}