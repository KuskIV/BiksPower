using Autofac;
using Autofac.Extensions.DependencyInjection;
using EnergyComparer;
using EnergyComparer.Services;
using System.Data;
using Serilog;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
    });


builder
    .UseServiceProviderFactory(new AutofacServiceProviderFactory())
    .ConfigureContainer<ContainerBuilder>((host, builder) =>
    {
        //builder.Register<IDbConnection>(f =>
        //{
        //    var connectionString = "CONNECTIONSTRING";
        //    var con = new MySql.Data.MySqlClient.MySqlConnection(connectionString);
        //    con.Open();
        //    return con;
        //}).As<IDbConnection>().AsSelf().InstancePerDependency().ExternallyOwned();

        builder.RegisterType<HardwareMonitorService>().As<IHardwareMonitorService>().SingleInstance().AutoActivate();
        builder.RegisterType<IntelPowerGadgetService>().As<IIntelPowerGadgetService>().SingleInstance().AutoActivate();
        builder.RegisterType<PrepareService>().As<IPrepareService>().SingleInstance().AutoActivate();
    })
    .UseSerilog((ctx, lc) => lc
    .WriteTo.Console());

var host = builder.Build();

await host.RunAsync();