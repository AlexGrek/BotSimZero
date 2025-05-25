// BroscriptParser.cs - A Bash-like object-oriented script language parser and executor for .NET 8

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace SimuliEngine.BroScript
{
    // ----------------------
    // AST Definitions
    // ----------------------
    public abstract class Expr
    {
        public abstract object Evaluate(ScriptContext context);
    }

    public class LiteralExpr : Expr
    {
        public object Value { get; }
        public LiteralExpr(object value) => Value = value;
        public override object Evaluate(ScriptContext context) => Value;
    }

    public class VariableExpr : Expr
    {
        public string Name { get; }
        public VariableExpr(string name) => Name = name;
        public override object Evaluate(ScriptContext context) => context.GetVariable(Name);
    }

    public class BinaryExpr : Expr
    {
        public Expr Left { get; }
        public string Operator { get; }
        public Expr Right { get; }
        public BinaryExpr(Expr left, string op, Expr right) => (Left, Operator, Right) = (left, op, right);

        public override object Evaluate(ScriptContext context)
        {
            var left = Left.Evaluate(context);
            var right = Right.Evaluate(context);
            return Operator switch
            {
                "+" => (dynamic)left + (dynamic)right,
                "-" => (dynamic)left - (dynamic)right,
                "*" => (dynamic)left * (dynamic)right,
                "/" => (dynamic)left / (dynamic)right,
                "==" => Equals(left, right),
                "!=" => !Equals(left, right),
                "&" => (bool)left && (bool)right,
                "|" => (bool)left || (bool)right,
                _ => throw new Exception($"Unknown operator {Operator}")
            };
        }
    }

    public class UnaryExpr : Expr
    {
        public string Operator { get; }
        public Expr Operand { get; }
        public UnaryExpr(string op, Expr operand) => (Operator, Operand) = (op, operand);
        public override object Evaluate(ScriptContext context) => Operator switch
        {
            "!" => !(bool)Operand.Evaluate(context),
            _ => throw new Exception($"Unknown unary operator {Operator}")
        };
    }

    public class MemberAccessExpr : Expr
    {
        public Expr Target { get; }
        public string MemberName { get; }
        public List<Expr>? Arguments { get; }

        public MemberAccessExpr(Expr target, string memberName, List<Expr>? args = null)
        {
            Target = target;
            MemberName = memberName;
            Arguments = args;
        }

        public override object Evaluate(ScriptContext context)
        {
            var target = Target.Evaluate(context);
            var type = target.GetType();
            if (Arguments == null)
            {
                var prop = type.GetProperty(MemberName);
                return prop?.GetValue(target) ?? throw new Exception($"Property {MemberName} not found");
            }
            else
            {
                var args = Arguments.Select(a => a.Evaluate(context)).ToArray();
                var method = type.GetMethods().FirstOrDefault(m => m.Name == MemberName && m.GetParameters().Length == args.Length);
                return method?.Invoke(target, args) ?? throw new Exception($"Method {MemberName} not found");
            }
        }
    }

    // ----------------------
    // ScriptContext and Command
    // ----------------------
    public class ScriptContext
    {
        public Dictionary<string, object> Variables = new();
        public object GetVariable(string name) => Variables.TryGetValue(name, out var v) ? v : throw new Exception($"Variable '{name}' not found");
    }

    public interface ICommand
    {
        (object? output, string? error, int exitCode) Execute(ScriptContext context, List<Expr> args);
    }

    public class GetCommand : ICommand
    {
        public (object?, string?, int) Execute(ScriptContext context, List<Expr> args)
        {
            try
            {
                var result = args.Select(arg => arg.Evaluate(context)).ToList();
                return (result, null, 0);
            }
            catch (Exception ex)
            {
                return (null, ex.Message, 1);
            }
        }
    }

    public class RunCommand : ICommand
    {
        public (object?, string?, int) Execute(ScriptContext context, List<Expr> args)
        {
            try
            {
                var result = args.Select(arg => arg.Evaluate(context)).LastOrDefault();
                return (result, null, 0);
            }
            catch (Exception ex)
            {
                return (null, ex.Message, 1);
            }
        }
    }

    public class SetCommand : ICommand
    {
        public (object?, string?, int) Execute(ScriptContext context, List<Expr> args)
        {
            if (args.Count != 2 || args[0] is not MemberAccessExpr memberExpr)
                return (null, "Set expects a member target and a value", 1);

            try
            {
                var target = memberExpr.Target.Evaluate(context);
                var type = target.GetType();
                var value = args[1].Evaluate(context);
                var prop = type.GetProperty(memberExpr.MemberName);
                if (prop == null) return (null, "Property not found", 1);
                prop.SetValue(target, value);
                return (value, null, 0);
            }
            catch (Exception ex)
            {
                return (null, ex.Message, 1);
            }
        }
    }

    public class BindCommand : ICommand
    {
        public (object?, string?, int) Execute(ScriptContext context, List<Expr> args)
        {
            if (args.Count != 2 || args[0] is not VariableExpr varExpr)
                return (null, "Bind expects a variable name and value", 1);
            var value = args[1].Evaluate(context);
            context.Variables[varExpr.Name] = value;
            return (value, null, 0);
        }
    }

    public class StrCommand : ICommand
    {
        public (object?, string?, int) Execute(ScriptContext context, List<Expr> args)
        {
            try
            {
                var result = args.Select(arg => arg.Evaluate(context)?.ToString()).ToList();
                return (result, null, 0);
            }
            catch (Exception ex)
            {
                return (null, ex.Message, 1);
            }
        }
    }

    public class DebugCommand : ICommand
    {
        public (object?, string?, int) Execute(ScriptContext context, List<Expr> args)
        {
            try
            {
                var result = args.Select(arg => DebugPrint(arg.Evaluate(context))).ToList();
                return (result, null, 0);
            }
            catch (Exception ex)
            {
                return (null, ex.Message, 1);
            }
        }

        private string DebugPrint(object obj)
        {
            if (obj is null) return "null";
            if (obj is IDictionary dict)
            {
                var pairs = dict.Cast<DictionaryEntry>().Select(e => $"{e.Key}: {DebugPrint(e.Value)}");
                return "{" + string.Join(", ", pairs) + "}";
            }
            if (obj is IEnumerable enumerable && obj is not string)
            {
                var items = enumerable.Cast<object>().Select(DebugPrint);
                return "[" + string.Join(", ", items) + "]";
            }
            var method = obj.GetType().GetMethod("Debug");
            return method != null ? method.Invoke(obj, null)?.ToString() ?? "null" : obj.ToString();
        }
    }

    public class ForeachCommand : ICommand
    {
        public (object?, string?, int) Execute(ScriptContext context, List<Expr> args)
        {
            if (args.Count < 3 || args[0] is not VariableExpr varExpr)
                return (null, "foreach expects a variable, a collection, and expressions", 1);

            var collectionObj = args[1].Evaluate(context);
            if (collectionObj is not IEnumerable enumerable)
                return (null, "Second argument must be iterable", 1);

            object? lastResult = null;
            string? error = null;
            int code = 0;

            foreach (var item in enumerable)
            {
                context.Variables[varExpr.Name] = item;
                foreach (var expr in args.Skip(2))
                {
                    try
                    {
                        lastResult = expr.Evaluate(context);
                    }
                    catch (Exception ex)
                    {
                        error = ex.Message;
                        code = 1;
                        break;
                    }
                }
                if (code != 0) break;
            }

            return (lastResult, error, code);
        }
    }

    // ----------------------
    // ScriptEngine
    // ----------------------
    public class ScriptEngine
    {
        private readonly Dictionary<string, ICommand> commands = new(StringComparer.OrdinalIgnoreCase)
        {
            { "get", new GetCommand() },
            { "run", new RunCommand() },
            { "set", new SetCommand() },
            { "bind", new BindCommand() },
            { "str", new StrCommand() },
            { "debug", new DebugCommand() },
            { "foreach", new ForeachCommand() },
        };

        public (object?, string?, int) Execute(string script, ScriptContext context)
        {
            try
            {
                var (command, args) = BroscriptParser.Parse(script);
                if (!commands.TryGetValue(command, out var cmd))
                    return (null, $"Unknown command '{command}'", 2);
                return cmd.Execute(context, args);
            }
            catch (Exception ex)
            {
                return (null, ex.Message, 2);
            }
        }
    }

    // ----------------------
    // Parser
    // ----------------------
    public static class BroscriptParser
    {
        public static (string command, List<Expr> args) Parse(string input)
        {
            var tokens = Tokenize(input);
            if (tokens.Count == 0) throw new Exception("Empty input");
            var command = tokens[0];
            var args = new List<Expr>();
            var index = 1;
            while (index < tokens.Count)
                args.Add(ParseExpr(tokens, ref index));
            return (command, args);
        }

        private static List<string> Tokenize(string input)
        {
            var tokens = new List<string>();
            var sb = new StringBuilder();
            bool inString = false;
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];

                if (inString)
                {
                    if (c == '\\' && i + 1 < input.Length && input[i + 1] == '"')
                    {
                        sb.Append('"');
                        i++;
                    }
                    else if (c == '"')
                    {
                        sb.Insert(0, '"');
                        sb.Append('"');
                        tokens.Add(sb.ToString());
                        sb.Clear();
                        inString = false;
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
                else if (char.IsWhiteSpace(c))
                {
                    if (sb.Length > 0)
                    {
                        tokens.Add(sb.ToString());
                        sb.Clear();
                    }
                }
                else if (c == '"')
                {
                    inString = true;
                    sb.Clear();
                }
                else if ("()+-*/=!|&.,".Contains(c))
                {
                    if (sb.Length > 0)
                    {
                        tokens.Add(sb.ToString());
                        sb.Clear();
                    }
                    tokens.Add(c.ToString());
                }
                else
                {
                    sb.Append(c);
                }
            }
            if (sb.Length > 0) tokens.Add(sb.ToString());
            return tokens;
        }

        private static Expr ParseExpr(List<string> tokens, ref int index)
        {
            Expr left = ParsePrimary(tokens, ref index);
            while (index < tokens.Count && "+-*/==!=|&".Contains(tokens[index]))
            {
                string op = tokens[index++];
                Expr right = ParsePrimary(tokens, ref index);
                left = new BinaryExpr(left, op, right);
            }
            return left;
        }

        private static Expr ParsePrimary(List<string> tokens, ref int index)
        {
            string token = tokens[index++];
            if (token == "(")
            {
                var e = ParseExpr(tokens, ref index);
                if (tokens[index++] != ")") throw new Exception("Expected )");
                return e;
            }
            if (token == "!")
                return new UnaryExpr(token, ParsePrimary(tokens, ref index));
            if (token.StartsWith("\""))
                return new LiteralExpr(token.Substring(1, token.Length - 2).Replace("\\\"", "\""));
            if (int.TryParse(token, out var i)) return new LiteralExpr(i);
            if (double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out var f)) return new LiteralExpr(f);
            if (token == "true") return new LiteralExpr(true);
            if (token == "false") return new LiteralExpr(false);
            if (token == "null") return new LiteralExpr(null);

            Expr expr = new VariableExpr(token);
            while (index < tokens.Count && tokens[index] == ".")
            {
                index++; // skip .
                string member = tokens[index++];
                if (index < tokens.Count && tokens[index] == "(")
                {
                    index++; // skip (
                    var args = new List<Expr>();
                    while (index < tokens.Count && tokens[index] != ")")
                    {
                        args.Add(ParseExpr(tokens, ref index));
                        if (index < tokens.Count && tokens[index] == ",") index++;
                    }
                    if (index >= tokens.Count || tokens[index++] != ")") throw new Exception("Expected )");
                    expr = new MemberAccessExpr(expr, member, args);
                }
                else
                {
                    expr = new MemberAccessExpr(expr, member);
                }
            }
            return expr;
        }
    }
}