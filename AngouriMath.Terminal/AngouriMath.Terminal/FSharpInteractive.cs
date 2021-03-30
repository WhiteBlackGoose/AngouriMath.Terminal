using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Events;
using Microsoft.DotNet.Interactive.Server;
using System.Diagnostics;
using System.Linq;
using static AngouriMath.Terminal.ExecutionResult;

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
            string? res = null;
            while (!process.StandardOutput.EndOfStream)
            {
                var line = process.StandardOutput.ReadLine();
                var des = KernelEventEnvelope.Deserialize(line).Event;
                if (des is DisplayEvent display)
                    res = display.FormattedValues.First().Value;
                if (des is CommandSucceeded)
                    return res is null ? new VoidSuccess() : new VerboseSuccess(res);
                if (des is CommandFailed failed)
                    return new Error(failed.Message);
            }
            return new EOF();
        }
    }

    public abstract record ExecutionResult
    {
        public sealed record SuccessPackageAdded : ExecutionResult;
        public sealed record Error(string Message) : ExecutionResult;
        public sealed record VoidSuccess : ExecutionResult;
        public sealed record VerboseSuccess(string Result) : ExecutionResult;
        public sealed record EOF : ExecutionResult;
    }
}
