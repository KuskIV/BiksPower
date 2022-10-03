using Autofac;
using Autofac.Extensions.DependencyInjection;
using EnergyComparer;
using EnergyComparer.Services;
using System.Data;
using Serilog;
using EnergyComparer.Repositories;
using EnergyComparer.Handlers;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
    });


builder
    .UseServiceProviderFactory(new AutofacServiceProviderFactory())
    .ConfigureContainer<ContainerBuilder>((host, builder) =>
    {

        builder.Register<Func<IDbConnection>>(f => () =>
        {
            var connectionString = host.Configuration.GetValue<string>("ConnectionString");
            var con = new MySql.Data.MySqlClient.MySqlConnection(connectionString);
            con.Open();
            return con;
        });


        //builder.Register<IDbConnection>(f =>
        //{
        //    var connectionString = host.Configuration.GetValue<string>("ConnectionString");
        //    var con = new MySql.Data.MySqlClient.MySqlConnection(connectionString);
        //    con.Open();
        //    return con;
        //}).As<IDbConnection>().AsSelf().InstancePerDependency().ExternallyOwned();

        builder.RegisterType<HardwareMonitorService>().As<IHardwareMonitorService>().SingleInstance().AutoActivate();
        builder.RegisterType<ExperimentService>().As<IExperimentService>().SingleInstance().AutoActivate();
        builder.RegisterType<HardwareHandler>().As<IHardwareHandler>().SingleInstance().AutoActivate();
        builder.RegisterType<InsertExperimentRepository>().As<IInsertExperimentRepository>().SingleInstance().AutoActivate();
        builder.RegisterType<GetExperimentRepository>().As<IGetExperimentRepository>().SingleInstance().AutoActivate();
        builder.RegisterType<DataHandler>().As<IDataHandler>().SingleInstance().AutoActivate();
    })
    .UseSerilog((ctx, lc) => lc
    .WriteTo.Console());

var host = builder.Build();

await host.RunAsync();