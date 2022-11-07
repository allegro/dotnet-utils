using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Allegro.Extensions.AspNetCore.Attributes;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Hosting;

namespace Allegro.Extensions.AspNetCore.Features
{
    /// <summary>
    /// Feature provider that removes controllers marked with <see cref="SkipOnProdAttribute"/> on environments
    /// other than test (Environment=dev|uat), unless HostEnvironment is Development. IsTestEnv can also be set explicitly.
    /// </summary>
    public class SkipControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {
        private readonly string[] _testEnvironments = { "dev", "uat" };
        private readonly bool _isTestEnv;

        /// <summary>
        /// Ctor to set test env with custom code
        /// </summary>
        /// <param name="isTestEnv"></param>
        public SkipControllerFeatureProvider(bool isTestEnv)
        {
            _isTestEnv = isTestEnv;
        }

        /// <summary>
        /// Ctor with default support of environment type based on environment variable convention
        /// </summary>
        /// <param name="aspNetEnvironment"></param>
        /// <param name="overrideTestEnvironments"></param>
        public SkipControllerFeatureProvider(
            IHostEnvironment aspNetEnvironment,
            string[]? overrideTestEnvironments = null)
        {
            var environment = Environment.GetEnvironmentVariable("Environment");
            _testEnvironments = overrideTestEnvironments ?? _testEnvironments;
            _isTestEnv =
                environment != null &&
                _testEnvironments.Contains(environment, StringComparer.InvariantCultureIgnoreCase);
            _isTestEnv = _isTestEnv || aspNetEnvironment.IsDevelopment();
        }

        /// <summary>
        /// Implementation of IApplicationFeatureProvider interface
        /// </summary>
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            if (_isTestEnv)
            {
                return;
            }

            var controllers = feature.Controllers.ToList();
            foreach (var controllerType in controllers)
            {
                var skipOnProdAttribute = controllerType.GetCustomAttribute<SkipOnProdAttribute>();
                if (skipOnProdAttribute != null)
                {
                    feature.Controllers.Remove(controllerType);
                }
            }
        }
    }
}