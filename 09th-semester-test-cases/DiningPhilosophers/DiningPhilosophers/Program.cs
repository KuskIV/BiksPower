using DiningPhilosophers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

var arguments = Environment.GetCommandLineArgs();

if (!int.TryParse(arguments[1], out var experimentDurationInMinutes))
{
    throw new Exception("The first argument should be an integer presenting the duration of the experiment in minutes");
}


var startTime = DateTime.UtcNow;
var counter = 0;


Console.WriteLine("Starting IdleCase");

var testCase = new TestCase();

while (startTime.AddMinutes(experimentDurationInMinutes) > DateTime.UtcNow)
{
    testCase.Run();
    counter += 1;
}

Console.WriteLine("Stopping DiningPhilolophers");
Console.WriteLine(counter);
