using Allegro.Extensions.Identifiers.AspNetCore.Swagger;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Allegro.Extensions.Identifiers.Demo
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            app
                .UseRouting()
                .UseSwagger()
                .UseSwaggerUI()
                .UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddControllers();

            services
                .AddStronglyTypedIds()
                .AddSwaggerGen();
        }
    }
}