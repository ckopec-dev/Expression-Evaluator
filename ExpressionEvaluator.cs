using System;
using System.Collections.Generic;
using System.Globalization;

public class ExpressionEvaluator
{
    private string _text;
    private int _pos;

    private Dictionary<string, double> _variables;
    private Dictionary<string, Func<double[], double>> _functions;

    public ExpressionEvaluator()
    {
        _variables = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        _functions = new Dictionary<string, Func<double[], double>>(StringComparer.OrdinalIgnoreCase);

        // Built-in functions
        _functions["sin"] = args => Math.Sin(args[0]);
        _functions["cos"] = args => Math.Cos(args[0]);
        _functions["tan"] = args => Math.Tan(args[0]);
        _functions["sqrt"] = args => Math.Sqrt(args[0]);
        _functions["log"] = args => Math.Log(args[0]);
        _functions["abs"] = args => Math.Abs(args[0]);
        _functions["max"] = args => Math.Max(args[0], args[1]);
        _functions["min"] = args => Math.Min(args[0], args[1]);

        // Constants
        _variables["pi"] = Math.PI;
        _variables["e"] = Math.E;
    }

    public void SetVariable(string name, double value)
    {
        _variables[name] = value;
    }

    public double Evaluate(string text)
    {
        _text = text;
        _pos = 0;

        double result = ParseExpression();
        SkipWhitespace();

        if (_pos < _text.Length)
            throw new Exception("Unexpected character at end.");

        return result;
    }

    // expression = term ((+|-) term)*
    private double ParseExpression()
    {
        double value = ParseTerm();

        while (true)
        {
            SkipWhitespace();

            if (Match('+'))
                value += ParseTerm();
            else if (Match('-'))
                value -= ParseTerm();
            else
                break;
        }

        return value;
    }

    // term = power ((*|/) power)*
    private double ParseTerm()
    {
        double value = ParsePower();

        while (true)
        {
            SkipWhitespace();

            if (Match('*'))
                value *= ParsePower();
            else if (Match('/'))
                value /= ParsePower();
            else
                break;
        }

        return value;
    }

    // power = factor (^ power)?   (right associative)
    private double ParsePower()
    {
        double value = ParseFactor();

        SkipWhitespace();
        if (Match('^'))
        {
            double exponent = ParsePower();
            value = Math.Pow(value, exponent);
        }

        return value;
    }

    // factor = (+|-) factor | number | variable | function | '(' expression ')'
    private double ParseFactor()
    {
        SkipWhitespace();

        if (Match('+'))
            return ParseFactor();

        if (Match('-'))
            return -ParseFactor();

        if (Match('('))
        {
            double value = ParseExpression();
            SkipWhitespace();

            if (!Match(')'))
                throw new Exception("Missing closing parenthesis.");

            return value;
        }

        if (char.IsLetter(Current))
            return ParseIdentifier();

        return ParseNumber();
    }

    private double ParseIdentifier()
    {
        string name = ParseName();

        SkipWhitespace();

        // Function call
        if (Match('('))
        {
            var args = new List<double>();

            if (!Match(')'))
            {
                do
                {
                    args.Add(ParseExpression());
                    SkipWhitespace();
                }
                while (Match(','));

                if (!Match(')'))
                    throw new Exception("Missing closing parenthesis in function.");
            }

            if (!_functions.TryGetValue(name, out var func))
                throw new Exception($"Unknown function: {name}");

            return func(args.ToArray());
        }

        // Variable
        if (!_variables.TryGetValue(name, out var value))
            throw new Exception($"Unknown variable: {name}");

        return value;
    }

    private string ParseName()
    {
        int start = _pos;

        while (_pos < _text.Length && char.IsLetterOrDigit(_text[_pos]))
            _pos++;

        return _text.Substring(start, _pos - start);
    }

    private double ParseNumber()
    {
        SkipWhitespace();

        int start = _pos;

        while (_pos < _text.Length &&
              (char.IsDigit(_text[_pos]) || _text[_pos] == '.'))
        {
            _pos++;
        }

        if (start == _pos)
            throw new Exception("Number expected.");

        string number = _text.Substring(start, _pos - start);
        return double.Parse(number, CultureInfo.InvariantCulture);
    }

    private void SkipWhitespace()
    {
        while (_pos < _text.Length && char.IsWhiteSpace(_text[_pos]))
            _pos++;
    }

    private bool Match(char c)
    {
        if (_pos < _text.Length && _text[_pos] == c)
        {
            _pos++;
            return true;
        }
        return false;
    }

    private char Current => _pos < _text.Length ? _text[_pos] : '\0';
}
