using Autofac;
using Autofac.Extensions.DependencyInjection;
using EnergyComparer;
using EnergyComparer.Services;
using System.Data;
using Serilog;
using EnergyComparer.Repositories;
using EnergyComparer.Handlers;
using ILogger = Serilog.ILogger;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
    });


builder
    .UseServiceProviderFactory(new AutofacServiceProviderFactory())
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        config.Sources.Clear();

        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("secrets/appsettings.secrets.json", true)
            .AddUserSecrets<Program>(true);

    })
    .ConfigureContainer<ContainerBuilder>((host, builder) =>
    {
        builder.Register<Func<IDbConnection>>(f => () =>
        {
            var connectionString = host.Configuration.GetValue<string>("ConnectionString");
            var con = new MySql.Data.MySqlClient.MySqlConnection(connectionString);
            con.Open();
            return con;
        });
        
        builder.RegisterType<ExperimentService>().As<IExperimentService>().SingleInstance().AutoActivate();
    })
    .UseSerilog((ctx, lc) => lc
    .WriteTo.Console());

var host = builder.Build();

await host.RunAsync();