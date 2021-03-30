using AngouriMath.Terminal;
using static AngouriMath.Terminal.ExecutionResult;
using System;

Console.WriteLine(
$@"
╔═══════════════════════════════════╗
║  Welcome to AngouriMath.Terminal  ║
╚═══════════════════════════════════╝
".Trim());

Console.WriteLine("Starting the kernel...");
var ui = new UserInterface();
var interactive = new FSharpInteractive();
var execRes = interactive.Execute("#r \"nuget: AngouriMath.FSharp, *-*\"");
if (!HandleResult(execRes)) return;
Console.WriteLine("1/3 done.");
var openRes = interactive.Execute(@"
open AngouriMath
open Core
open Functions
open Operators
open Shortcuts
");
if (!HandleResult(openRes)) return;
Console.WriteLine("2/3 done.");
Console.WriteLine("3/3 done. Started.");

while (true)
{
    var input = ui.ReadLine();
    if (input is null)
        continue;
    switch (interactive.Execute(input))
    {
        case VerboseSuccess(var text):
            ui.WriteLine(text);
            break;
        case Error(var message):
            ui.WriteLineError(message);
            break;
    }
}

static bool HandleResult(ExecutionResult res)
{
    if (res is Error(var message))
    {
        Console.WriteLine($"Error: {message}");
        Console.WriteLine($"Report about it to the official repo. The terminal will be closed.");
        Console.ReadLine();
        return false;
    }
    return true;
}