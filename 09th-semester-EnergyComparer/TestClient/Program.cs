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
using System.Security.Cryptography.Xml;
using TestClient.E3Experiment;


//var intel = new IntelPowerGadget();

//var data = intel.ParseData("C:\\Users\\Mads Kusk\\Documents\\09-experiment-data\\IntelPowerGadget\\2022-10-17-09-30-24.csv", 1, DateTime.UtcNow);

/*var e3 = new E3();
await e3.WaitForStart(DateTime.UtcNow);
await e3.WaitForStop();*/


//e3.Start(DateTime.UtcNow);
//await Task.Delay(10000 * 6);
//e3.Stop();

//E3Experiments E3Ex = new();
////await E3Ex.Experiment1("Ex1",90);
//await E3Ex.Experiment9("Ex9", 300, 10);
//Console.ReadLine();


//var hwm = new HardwareMonitor();

//hwm.Start(DateTime.UtcNow);

//await Task.Delay(TimeSpan.FromSeconds(5));
//hwm.Stop();