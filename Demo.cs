class Program
{
    static void Main()
    {
        var eval = new ExpressionEvaluator();

        eval.SetVariable("x", 10);
        eval.SetVariable("y", 5);

        Console.WriteLine(eval.Evaluate("x + y * 2"));            // 20
        Console.WriteLine(eval.Evaluate("sin(pi / 2)"));          // 1
        Console.WriteLine(eval.Evaluate("sqrt(16) + max(3, 7)")); // 11
        Console.WriteLine(eval.Evaluate("2^3^2"));                // 512
        Console.WriteLine(eval.Evaluate("-x + 3"));               // -7
    }
}
