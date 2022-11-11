using BinaryTrees;

var name = "BinaryTrees";

var arguments = Environment.GetCommandLineArgs();

if (!int.TryParse(arguments[1], out var experimentDurationInMinutes))
{
    throw new Exception("The first argument should be an integer presenting the duration of the experiment in minutes");
}


var startTime = DateTime.UtcNow;
var counter = 0;
var iterations = 10;
var testCase = new TestCase();

Console.WriteLine($"Starting {name}");

while (startTime.AddMinutes(experimentDurationInMinutes) > DateTime.UtcNow)
{
    testCase.Run(iterations);
    counter += 1;
}

Console.WriteLine($"Stopping {name}");
Console.WriteLine(counter);

