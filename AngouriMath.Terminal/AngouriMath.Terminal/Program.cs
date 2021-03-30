using AngouriMath.Terminal;
using static AngouriMath.Terminal.ExecutionResult;
using System;

Console.WriteLine(
$@"
══════════════════════════════════════════════════════════════════════
                Welcome to AngouriMath.Terminal.

It is an interface to AngouriMath, open source symbolic algebra
library. The terminal uses F# Interactive inside, so that you can
run any command you could in normal F#. AngouriMath.FSharp is
being installed every start, so you are guaranteed to be on the
latest version of it. Type 'preRunCode' to see, what code
was preran before you were able to type.
══════════════════════════════════════════════════════════════════════
".Trim());

Console.Write("Starting the kernel... ");
var ui = new UserInterface();
var interactive = new FSharpInteractive();
var preRunCode = "#r \"nuget: AngouriMath.FSharp, *-*\"";
preRunCode +=
@"
open AngouriMath
open Core
open Operators
open Shortcuts
open Constants
open Functions

let ( + ) a b =
    ((parsed a) + (parsed b)).InnerSimplified

let ( - ) a b =
    ((parsed a) - (parsed b)).InnerSimplified

let ( * ) a b =
    ((parsed a) * (parsed b)).InnerSimplified

let ( / ) a b =
    ((parsed a) / (parsed b)).InnerSimplified

let ( ** ) a b =
    ((parsed a).Pow(parsed b)).InnerSimplified

let x = symbol ""x""
let y = symbol ""y""
let a = symbol ""a""
let b = symbol ""b""
";
preRunCode += $"let preRunCode = \"{preRunCode.Replace("\"", "\\\"")}\"";
if (!HandleResult(interactive.Execute(preRunCode))) return;
Console.WriteLine("started. You can start working.");

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