using Allegro.Extensions.Identifiers.AspNetCore.Swagger;

namespace Allegro.Extensions.Identifiers.Demo
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