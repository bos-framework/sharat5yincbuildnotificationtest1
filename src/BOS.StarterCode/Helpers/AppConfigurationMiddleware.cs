using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BOS.StarterCode.Helpers.HttpClientFactories;
using BOS.StarterCode.Helpers.MultiTenancy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BOS.StarterCode.Helpers
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class AppConfigurationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        public IConfiguration _configuration { get; }

        public AppConfigurationMiddleware(RequestDelegate next, ILoggerFactory logFactory, IConfiguration configuration, IHttpContextAccessor contextAccessor)
        {
            _next = next;
            _configuration = configuration;
            _contextAccessor = contextAccessor;
            _logger = logFactory.CreateLogger("MyMiddleware");
        }

        public async Task Invoke(HttpContext httpContext)
        {
            string appPath = string.Format("{0}://{1}", "https", httpContext.Request.Host);
            SetApplicationDefaultConfiguration(appPath);
            await _next(httpContext); // calling next middleware
        }

        public void SetApplicationDefaultConfiguration(string appPath)
        {
            bool isFetch = true;
            var Config = _contextAccessor.HttpContext.Session.GetString("ApplicationConfig");
            if (Config != null)
            {
                var AppConfig = JsonConvert.DeserializeObject<WhiteLabel>(Config);
                if (AppConfig != null && AppConfig.URL.Contains(appPath))
                {
                    isFetch = false;
                }
            }
            if (isFetch)
            {
                ApplicationSettings.ApiBaseUrl = _configuration["BOS:ServiceBaseURL"] + "" + _configuration["BOS:MultiTenancyRelativeURL"];
                var result = ApiClientFactory.Instance.GetApplicationConfiguration(appPath);
                if (result != null && result.Count > 0)
                {
                    var ConfigData = result.FirstOrDefault();
                    _contextAccessor.HttpContext.Session.SetString("ApplicationConfig", JsonConvert.SerializeObject(ConfigData));
                }
            }
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class MyMiddlewareExtensions
    {
        public static IApplicationBuilder UseAppConfigurationMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AppConfigurationMiddleware>();
        }
    }
}
