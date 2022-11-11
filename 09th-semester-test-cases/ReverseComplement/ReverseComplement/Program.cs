using ReverseComplement;

var name = "ReverseComplement";

//var arguments = Environment.GetCommandLineArgs();

//if (!int.TryParse(arguments[1], out var experimentDurationInMinutes))
//{
//    throw new Exception("The first argument should be an integer presenting the duration of the experiment in minutes");
//}

var experimentDurationInMinutes = 1;


var startTime = DateTime.UtcNow;
var counter = 0;

Console.WriteLine($"Starting {name}");
var stopTime = startTime.AddMinutes(experimentDurationInMinutes);

counter = TestCase.Run(stopTime);

//while (stopTime > DateTime.UtcNow)
//{
//    Console.WriteLine("d");
//    counter += 1;
//}

Console.WriteLine($"Stopping {name}");
Console.WriteLine(counter);

