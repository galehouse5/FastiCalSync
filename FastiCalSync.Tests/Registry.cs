using Microsoft.Extensions.Configuration;
using System.IO;

namespace FastiCalSync.Tests
{
    public static class Registry
    {
        private static IConfigurationRoot appSettings;
        public static IConfigurationRoot AppSettings
        {
            get
            {
                if (appSettings == null)
                {
                    IConfigurationBuilder builder = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    appSettings = builder.Build();
                }

                return appSettings;
            }
        }
    }
}
