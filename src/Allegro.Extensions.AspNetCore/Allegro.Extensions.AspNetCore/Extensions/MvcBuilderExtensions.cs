using Allegro.Extensions.AspNetCore.Attributes;
using Allegro.Extensions.AspNetCore.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Allegro.Extensions.AspNetCore.Extensions
{
    public static class MvcBuilderExtensions
    {
        /// <summary>
        /// Configures the <see cref="SkipOnProdAttribute"/> based on `Environment` env variable and ASP.NET hosting environment.
        /// Assumes that `IsTestEnv = Environment == dev|uat || HostEnvironment.IsDevelopment()`.
        /// </summary>
        /// <param name="mvcBuilder">IMvcBuilder - target of extension method</param>
        /// <param name="aspNetEnvironment">Hosting environment</param>
        /// <param name="overrideTestEnvironments">Allows to override default test environments (dev|uat)</param>
        public static IMvcBuilder AddSkipOnProd(
            this IMvcBuilder mvcBuilder,
            IHostEnvironment aspNetEnvironment,
            string[]? overrideTestEnvironments = null)
        {
            return mvcBuilder
                .ConfigureApplicationPartManager(mgr =>
                {
                    mgr.FeatureProviders.Add(
                        new SkipControllerFeatureProvider(
                            aspNetEnvironment,
                            overrideTestEnvironments));
                });
        }

        /// <summary>
        /// Configures the <see cref="SkipOnProdAttribute"/>.
        /// </summary>
        /// <param name="mvcBuilder">IMvcBuilder - target of extension method</param>
        /// <param name="isTestEnv">Is currently running on test environment</param>
        public static IMvcBuilder AddSkipOnProd(this IMvcBuilder mvcBuilder, bool isTestEnv)
        {
            return mvcBuilder
                .ConfigureApplicationPartManager(mgr =>
                {
                    mgr.FeatureProviders.Add(new SkipControllerFeatureProvider(isTestEnv));
                });
        }
    }
}