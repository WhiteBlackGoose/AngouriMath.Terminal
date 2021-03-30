using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Events;
using Microsoft.DotNet.Interactive.Server;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using static AngouriMath.Terminal.ExecutionResult;
using Microsoft.DotNet.Interactive.FSharp;
using System;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Formatting;

namespace AngouriMath.Terminal
{
    internal sealed class FSharpInteractive
    {
        private readonly CompositeKernel compositeKernel;
        private readonly FSharpKernel kernel;
        public FSharpInteractive()
        {
            compositeKernel = new();
            kernel = new FSharpKernel()
                    .UseDefaultFormatting()
                    .UseNugetDirective()
                    .UseKernelHelpers()
                    .UseWho()
                    .UseDotNetVariableSharing()
                    ;
            compositeKernel.Add(kernel);
            Formatter.SetPreferredMimeTypeFor(typeof(object), "text/plain");
            Formatter.Register<object>(o => o.ToString());
        }

        public ExecutionResult Execute(string code)
        {
            var submitCode = new SubmitCode(code);
            string? nonVoidResponse = null;
            ExecutionResult? res = null;
            var computed = kernel.SendAsync(new SubmitCode(code)).Result;
            computed.KernelEvents.Subscribe(
                e =>
                {
                    switch (e)
                    {
                        case CommandSucceeded:
                            res = nonVoidResponse is null ? new VoidSuccess() : new VerboseSuccess(nonVoidResponse);
                            break;
                        case CommandFailed failed:
                            res = new Error(failed.Message);
                            break;
                        case DisplayEvent display:
                            nonVoidResponse = display.Value.ToString();
                            break;
                    }
                });
            return res ?? new EOF();
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
