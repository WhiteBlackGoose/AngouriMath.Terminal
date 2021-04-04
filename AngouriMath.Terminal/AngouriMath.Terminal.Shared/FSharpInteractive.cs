using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Events;
using Microsoft.DotNet.Interactive.Server;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using static AngouriMath.Terminal.Shared.ExecutionResult;
using Microsoft.DotNet.Interactive.FSharp;
using System;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Formatting;
using System.Text;
using System.Text.Json;

namespace AngouriMath.Terminal.Shared
{
    public sealed class FSharpInteractive
    {
        private const string ENC_PLAIN_PREFIX = "encp";
        private const string ENC_LATEX_PREFIX = "encl";

        private static string ObjectEncode(object o)
            => o switch
            {
                Core.ILatexiseable iLatex =>
                    ENC_LATEX_PREFIX + JsonSerializer.Serialize(new LatexSuccess(iLatex.Latexise(), o.ToString() ?? "")),
                _ =>
                    ENC_PLAIN_PREFIX + JsonSerializer.Serialize(new PlainTextSuccess(o.ToString() ?? ""))
            };

        private static ExecutionResult ObjectDecode(string? inp)
            => inp switch
            {
                null => new VoidSuccess(),
                var plain when plain.StartsWith(ENC_PLAIN_PREFIX)
                    => JsonSerializer.Deserialize<PlainTextSuccess>(plain[ENC_PLAIN_PREFIX.Length..]) ?? throw new NullReferenceException(),
                var latex when latex.StartsWith(ENC_LATEX_PREFIX)
                    => JsonSerializer.Deserialize<LatexSuccess>(latex[ENC_LATEX_PREFIX.Length..]) ?? throw new NullReferenceException(),
                _ => new VoidSuccess()
            };

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
            Formatter.Register<object>(ObjectEncode);
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
                            res = ObjectDecode(nonVoidResponse);
                            break;
                        case CommandFailed failed:
                            res = new Error(failed.Message);
                            break;
                        case DisplayEvent display:
                            nonVoidResponse = display.FormattedValues.First().Value;
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
        public sealed record PlainTextSuccess(string Result) : ExecutionResult;
        public sealed record LatexSuccess(string Latex, string Source) : ExecutionResult;
        public sealed record EOF : ExecutionResult;
    }
}

namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}