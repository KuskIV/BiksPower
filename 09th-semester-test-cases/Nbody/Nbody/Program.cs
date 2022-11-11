using Nbody;

var name = "Nbody";

var arguments = Environment.GetCommandLineArgs();

if (!int.TryParse(arguments[1], out var experimentDurationInMinutes))
{
    throw new Exception("The first argument should be an integer presenting the duration of the experiment in minutes");
}


var startTime = DateTime.UtcNow;
var counter = 0;

Console.WriteLine($"Starting {name}");
var testCase = new NBody();
var iterations = 10000;

while (startTime.AddMinutes(experimentDurationInMinutes) > DateTime.UtcNow)
{
    testCase.Start(iterations);
    counter += 1;
}

Console.WriteLine($"Stopping {name}");
Console.WriteLine(counter);

