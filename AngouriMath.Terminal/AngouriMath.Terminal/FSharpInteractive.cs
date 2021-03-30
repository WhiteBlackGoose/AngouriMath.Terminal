﻿using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Events;
using Microsoft.DotNet.Interactive.Server;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngouriMath.Terminal
{
    internal sealed class FSharpInteractive
    {
        private readonly Process process;
        public FSharpInteractive()
        {
            var info = new ProcessStartInfo("dotnet", "interactive stdio --default-kernel fsharp")
            {
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            process = new Process() { StartInfo = info };
            process.Start();
        }

        public ExecutionResult Execute(string code)
        {
            var submitCode = new SubmitCode(code);
            var envelope = KernelCommandEnvelope.Create(submitCode);
            var serialized = KernelCommandEnvelope.Serialize(envelope);
            process.StandardInput.WriteLine(serialized);
            while (!process.StandardOutput.EndOfStream)
            {
                var line = process.StandardOutput.ReadLine();
                var des = KernelEventEnvelope.Deserialize(line).Event;
                if (des is DisplayEvent display)
                    return new ExecutionResult.ToDisplay(display.FormattedValues.First().Value);
                if (des is CommandSucceeded)
                    return new ExecutionResult.Void();
                if (des is CommandFailed failed)
                    return new ExecutionResult.Error(failed.Message);
                if (des is PackageAdded)
                    return new ExecutionResult.PackageAdded();
            }
            return new ExecutionResult.EOF();
        }
    }

    public abstract record ExecutionResult
    {
        public sealed record ToDisplay(string Text) : ExecutionResult;
        public sealed record PackageAdded : ExecutionResult;
        public sealed record Error(string Message) : ExecutionResult;
        public sealed record Void : ExecutionResult;
        public sealed record EOF : ExecutionResult;
    }
}