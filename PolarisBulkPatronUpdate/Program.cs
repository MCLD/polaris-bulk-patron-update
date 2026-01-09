using Clc.Polaris.Api;
using Clc.Polaris.Api.Configuration;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PolarisBulkPatronUpdate;
using Serilog;

var app = new CommandLineApplication();

app.HelpOption();

var csv = app.Option("-c|--csv",
    "Path to CSV file to read fields from",
    CommandOptionType.SingleValue);
var go = app.Option("-g|--go",
    "Perform the action, without this it is a test run",
    CommandOptionType.NoValue);
var delay = app.Option("-d|--delay",
    "Delay between write operations to Polaris API in milliseconds",
    CommandOptionType.SingleValue);

app.OnExecuteAsync(async cancellationToken =>
{
    try
    {
        IConfigurationRoot? config = null;
        try
        {
            config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .AddUserSecrets<Program>()
                .Build();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Critical error during configuration: {ex.Message}");
        }

        if (config == null)
        {
            throw new ApplicationException("Could not build configuration object.");
        }

        try
        {
            Log.Logger = LogBuilder.Build(config).CreateLogger();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Critical error configuring logging: {ex.Message}");
        }

        string? csvValue = csv.HasValue() ? csv.Value() : null;

        if (string.IsNullOrEmpty(csvValue))
        {
            Log.Logger.Fatal("You must supply a CSV file with -c or --csv");
            return;
        }

        if (!File.Exists(csvValue))
        {
            Log.Logger.Fatal("Unable to find file: {Filename}", csvValue);
            return;
        }

        var papiSettings = new PapiSettings();
        config.GetSection(PapiSettings.SECTION_NAME).Bind(papiSettings);

        var configured = !string.IsNullOrEmpty(papiSettings.AccessKey)
            && !string.IsNullOrEmpty(papiSettings.AccessKey)
            && !string.IsNullOrEmpty(papiSettings.Hostname)
            && papiSettings.PolarisOverrideAccount != null;

        if (go.Values.Any() && !configured)
        {
            Log.Logger.Warning("Command line says to execute but PapiSettings not present in configuration, doing test run.");
        }

        var services = new ServiceCollection();
        services.AddHttpClient<IPapiClient, PapiClient>();
        services.AddLogging(_ => _.AddSerilog());
        services.AddSingleton<IPapiSettings>(papiSettings);
        services.AddSingleton<UpdateFields>();

        var runtimeConfig = new RuntimeConfiguration()
        {
            CsvPath = csvValue,
            Go = go.Values.Any() && configured
        };

        if (!string.IsNullOrEmpty(delay.Value())
            && int.TryParse(delay.Value(), out int delayResult))
        {
            runtimeConfig.DelayBetweenWrites = delayResult;
        }

        services.AddSingleton(runtimeConfig);

        var serviceProvider = services.BuildServiceProvider();

        await serviceProvider
            .GetRequiredService<UpdateFields>()
            .GoAsync(cancellationToken);
    }
    catch (Exception ex)
    {
        Log.Logger.Fatal(ex, "Critical error occurred: {Message}", ex.Message);
    }
    finally
    {
        await Log.CloseAndFlushAsync();
    }
});

return app.Execute(args);
