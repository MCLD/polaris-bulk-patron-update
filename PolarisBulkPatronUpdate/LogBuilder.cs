using System.Reflection;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Settings.Configuration;

namespace PolarisBulkPatronUpdate
{
    internal static class LogBuilder
    {
        internal static LoggerConfiguration Build(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            // these options are for single-file deployments
            var configurationOptions = new ConfigurationReaderOptions(
                typeof(ConsoleLoggerConfigurationExtensions).Assembly,
                typeof(FileLoggerConfigurationExtensions).Assembly,
                typeof(SeqLoggerConfigurationExtensions).Assembly);

            return new LoggerConfiguration()
                .WriteTo.Console()
                .ReadFrom.Configuration(config, configurationOptions)
                .Enrich.WithProperty(Enrichment.Application,
                    Assembly.GetExecutingAssembly().GetName().Name)
                .Enrich.WithProperty(Enrichment.MachineName, Environment.MachineName)
                .Enrich.WithProperty(Enrichment.Version, Assembly.GetEntryAssembly()?
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                    .InformationalVersion ?? "Unknown")
                .Enrich.FromLogContext();
        }

        internal static class Enrichment
        {
            public static readonly string Application = nameof(Application);
            public static readonly string MachineName = nameof(MachineName);
            public static readonly string Version = nameof(Version);
        }
    }
}
