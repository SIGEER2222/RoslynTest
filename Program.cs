string src = @"
using System;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine(""Hello, World!"");
    }
}
        ";

var run = new CodeRunner();
await run.RunCode(src);

Console.ReadLine();