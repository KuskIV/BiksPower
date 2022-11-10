using EnergyComparer;
using Serilog;
using ILogger = Serilog.ILogger;

public class Program
{
    private static ILogger _logger;
    private static IConfigurationRoot _configuration;

    private static async Task Main(string[] args)
    {
        //await _dataHandler.IncrementVersionForSystem(); // TODO: increment for all systems, not just the current one
        InitializeLogger();
        InitializeConfig();
        
        var worker = new Worker(_logger, _configuration);
        var cts = new CancellationToken();

        try
        {
            await worker.ExecuteAsync(cts);
        }
        catch (Exception e)
        {
            await worker.EnableWifi();
            _logger.Error(e, "Exception when running experiments");
            throw;
        }
        finally
        {
            Console.WriteLine("Press enter to close...");
            Console.ReadLine();
        }
    }

    private static void InitializeLogger()
    {
        _logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .WriteTo.Console()
        .CreateLogger();
    }

    private static void InitializeConfig()
    {
        _configuration = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .AddJsonFile("secrets/appsettings.secrets.json", true)
            .AddJsonFile("appsettings.json", true)
            .Build();
    }
}

//    var builder = Host.CreateDefaultBuilder(args)
//.ConfigureServices(services =>
//{
//    services.AddHostedService<Worker>();
//});

//    builder
//        .UseServiceProviderFactory(new AutofacServiceProviderFactory())
//        .ConfigureAppConfiguration((hostingContext, config) =>
//        {
//            config.Sources.Clear();

//            config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
//                .AddJsonFile("secrets/appsettings.secrets.json", true)
//                .AddUserSecrets<Program>(true);

//        })
//        .UseSerilog((ctx, lc) => lc
//        .WriteTo.Console());

//    var host = builder.Build();

//    await host.RunAsync();