using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.ComponentModel;
using System.Reflection;
using System.Text;

public class CodeRunner
{
    public static MetadataReference[] DefaultMetadataReferences = new MetadataReference[]
    {
        MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
        MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
        MetadataReference.CreateFromFile(typeof(List<>).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(int).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(DescriptionAttribute).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(Dictionary<,>).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(INotifyPropertyChanged).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(System.Linq.Expressions.Expression).Assembly.Location)
    };

    public async Task<string> RunCode(string code, string read = "")
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(code);
        string assemblyName = Path.GetRandomFileName();
        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName,
            syntaxTrees: new[] { syntaxTree },
            references: DefaultMetadataReferences,
            options: new CSharpCompilationOptions(OutputKind.ConsoleApplication));

        using (var ms = new MemoryStream())
        {
            EmitResult result = compilation.Emit(ms);

            if (!result.Success)
            {
                IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                    diagnostic.IsWarningAsError ||
                    diagnostic.Severity == DiagnosticSeverity.Error);

                StringBuilder sb = new StringBuilder();
                foreach (Diagnostic diagnostic in failures)
                {
                    sb.AppendLine($"{diagnostic.Id}: {diagnostic.GetMessage()}");
                }
                return sb.ToString();
            }
            else
            {
                ms.Seek(0, SeekOrigin.Begin);
                Assembly assembly = Assembly.Load(ms.ToArray());

                var entry = assembly.EntryPoint;
                var instance = assembly.CreateInstance(entry.Name);
                var output = entry.Invoke(instance, new object[] { new string[] { } });

                return output?.ToString();
            }
        }
    }
}
