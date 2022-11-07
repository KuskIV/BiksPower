
using Microsoft.VisualBasic;

var arguments = Environment.GetCommandLineArgs();

if (!int.TryParse(arguments[1], out var experimentDurationInMinutes))
{
    throw new Exception("The first argument should be an integer presenting the duration of the experiment in minutes");
}


var startTime = DateTime.UtcNow;
var counter = 0;

Console.WriteLine("Starting IdleCase");

while (startTime.AddMinutes(experimentDurationInMinutes) > DateTime.UtcNow)
{
    Thread.Sleep(TimeSpan.FromSeconds(30));
    counter += 1;
}

Console.WriteLine("Stopping IdleCase");
Console.WriteLine(counter);
