using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System;

namespace NETCore_DLL_Runtime_BuildAndLoad
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var assemblyName = "RuntimeAssemblyTest";

            var references = new List<MetadataReference>
            {
                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location)
            };

            var code = @"
            using System;

            namespace RuntimeAssemblyTest
            {
                internal struct TestStruct
                {
                    public int Property1 { get; set; }
                    public int Property2 { get; set; }
                }

                public class TestClass
                {
                    public bool Property1 { get; set; }
                    public bool Property2 { get; set; }
                }
            }";

            Console.WriteLine("Start build...");

            if (!Build(references, code, assemblyName, out List<Diagnostic> errors))
            {
                Console.WriteLine("Build errors:");

                foreach (var error in errors)
                    Console.WriteLine($"\t{error}");
            }
            else
            {
                Console.WriteLine("Build success!");

                Console.WriteLine("Loading assembly...");

                Load(assemblyName);
            }

            Console.ReadLine();
        }

        private static bool Build(List<MetadataReference> references, string code, string assemblyName, out List<Diagnostic> errors)
        {
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(code);

            var compilation = CSharpCompilation.Create(
                assemblyName: assemblyName,
                syntaxTrees: new[] { syntaxTree },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            if (File.Exists($"{assemblyName}.dll"))
                File.Delete($"{assemblyName}.dll");

            using (var file = new FileStream($"{assemblyName}.dll", FileMode.CreateNew))
            {
                EmitResult result = compilation.Emit(file);

                if (!result.Success)
                {
                    errors = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error)
                        .ToList();
                }
                else
                {
                    errors = null;
                }
            }

            return errors == null;
        }

        private static void Load(string assemblyName)
        {
            var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.GetFullPath($"{assemblyName}.dll"));

            Type[] types = assembly.GetTypes();

            foreach (var type in types)
                Console.WriteLine($"Type found: {type.FullName}");
        }
    }
}