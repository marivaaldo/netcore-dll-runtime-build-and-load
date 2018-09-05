using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using System;
using System.Reflection;

namespace NETCore_DLL_Runtime_BuildAndLoad
{
    public static class Helpers
    {
        public static bool Build(List<MetadataReference> references, SyntaxTree syntaxTree, string assemblyName, out List<Diagnostic> errors, SyntaxTree assemblyProperties = null)
        {
            var syntaxTrees = new List<SyntaxTree>() { syntaxTree };

            if (assemblyProperties != null)
                syntaxTrees.Add(assemblyProperties);

            var compilation = CSharpCompilation.Create(
                assemblyName: assemblyName,
                syntaxTrees: syntaxTrees,
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

        public static bool Build(List<MetadataReference> references, string code, string assemblyName, out List<Diagnostic> errors, string assemblyProperties = null)
        {
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(code);
            var assemblyPropertiesTree = SyntaxFactory.ParseSyntaxTree(assemblyProperties);

            return Build(references, syntaxTree, assemblyName, out errors, assemblyPropertiesTree);
        }

        public static Assembly Load(string assemblyName)
        {
            return AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.GetFullPath($"{assemblyName}.dll"));
        }
    }
}