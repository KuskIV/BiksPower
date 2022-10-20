// See https://aka.ms/new-console-template for more information


using CsvHelper;
using EnergyComparer.Models;
using EnergyComparer.Profilers;
using EnergyComparer.Services;
using Microsoft.AspNetCore.Routing;
using Serilog.Core;
using System.Formats.Asn1;
using System.Globalization;
using System.IO;


//var intel = new IntelPowerGadget();

//var data = intel.ParseCsv("C:\\Users\\Mads Kusk\\Documents\\09-experiment-data\\IntelPowerGadget\\2022-09-30-12-20-15.csv", 1, DateTime.UtcNow);

var e3 = new E3();
e3.Start(DateTime.UtcNow);
await Task.Delay(10000 * 6);
e3.Stop();

/*var service = new HardwareMonitorService(null);

service.GetCoreTemperatures();

hwm.Start(DateTime.UtcNow);

await Task.Delay(TimeSpan.FromSeconds(20));
Console.WriteLine("retrying");
hwm.Stop();

while (true)
{
    await Task.Delay(1000);
}

Console.WriteLine("Hello, World!");*/
