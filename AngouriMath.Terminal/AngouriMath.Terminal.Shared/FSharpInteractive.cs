﻿using Microsoft.DotNet.Interactive.Commands;
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
using System.Threading.Tasks;
using System.Reactive.Linq;

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

        private FSharpKernel kernel;

        private async Task<ExecutionResult> LoadAssembly(string path)
        {
            var loc = path.Replace(@"\", @"\\");
            return await Execute($"#r \"{loc}\"");
        }

        public static async Task<DUnion<Error, FSharpInteractive>> Create()
        {
            var interactive = new FSharpInteractive();
            interactive.kernel = new FSharpKernel();
            
            if (await interactive.LoadAssembly(typeof(MathS).Assembly.Location) is Error err1)
                return new(err1);
            if (await interactive.LoadAssembly(typeof(FSharp.Core).Assembly.Location) is Error err2)
                return new(err2);
            if (await interactive.LoadAssembly(typeof(Interactive.AggressiveOperators).Assembly.Location) is Error err3)
                return new(err3);

            Formatter.SetPreferredMimeTypeFor(typeof(object), "text/plain");
            Formatter.Register<object>(ObjectEncode);

            return new(interactive);
        }

        private FSharpInteractive() { }

        public async Task<ExecutionResult> Execute(string code)
        {
            var submitCode = new SubmitCode(code);
            string? nonVoidResponse = null;
            ExecutionResult? res = null;
            var computed = await kernel.SendAsync(new SubmitCode(code));
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

    public sealed record DUnion<T0, T1> where T0 : class where T1 : class
    {
        readonly T0? field0 = default;
        readonly T1? field1 = default;
        int type;
        public DUnion(T0 value) => (type, field0) = (0, value);
        public DUnion(T1 value) => (type, field1) = (1, value);

        public T? As<T>() where T : class
        {
            if (type == 0 && field0 is T t0)
                return t0;
            if (type == 1 && field1 is T t1)
                return t1;
            return null;
        }
    }
}

namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}