using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngouriMath.Terminal.Shared
{
    public static class CodeSnippets
    {
        public const string AngouriMathInstall = "#r \"nuget: AngouriMath.FSharp, *-*\"";
        public const string OpensAndOperators =
@"
open AngouriMath
open Core
open Operators
open Shortcuts
open Constants
open Functions

let eval (x : obj) = 
    match (parsed x).InnerSimplified with
    | :? Entity.Number.Integer as i -> i.ToString()
    | :? Entity.Number.Rational as i -> i.RealPart.EDecimal.ToString()
    | :? Entity.Number.Real as re -> re.RealPart.EDecimal.ToString()
    | :? Entity.Number.Complex as cx -> cx.RealPart.EDecimal.ToString() + "" + "" + cx.ImaginaryPart.EDecimal.ToString() + ""i""
    | other -> (evaled other).ToString()

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
    }
}
