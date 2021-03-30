using AngouriMath.Terminal;
using static AngouriMath.Terminal.ExecutionResult;
using System;

Console.WriteLine("Starting the core...");
var interactive = new FSharpInteractive();
interactive.Execute("#r \"nuget: AngouriMath.FSharp, *-*\"");
Console.WriteLine("Installed");

while (true)
{
    var input = Console.ReadLine();
    if (input is null)
        continue;
    var response = interactive.Execute(input) switch
    {
        ToDisplay display => display.Text,
        PackageAdded => "Package added",
        ExecutionResult.Void => "",
        Error error => $"Error: {error}",
        EOF => "",
        _ => throw new Exception()
    };
    Console.WriteLine(response);
}