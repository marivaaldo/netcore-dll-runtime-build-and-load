using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

            Console.WriteLine("Start build...");

            //var build = GetCode();
            var build = GetSyntax();

            if (!Helpers.Build(references, build, assemblyName, out List<Diagnostic> errors))
            {
                Console.WriteLine("Build errors:");

                foreach (var error in errors)
                    Console.WriteLine($"\t{error}");
            }
            else
            {
                Console.WriteLine("Build success!");

                Console.WriteLine("Loading assembly...");

                var assembly = Helpers.Load(assemblyName);

                Type[] types = assembly.GetTypes();

                foreach (var type in types)
                {
                    Console.WriteLine($"Type found: {type.FullName}");

                    foreach (var property in type.GetProperties())
                    {
                        Console.WriteLine($"\tProperty found: {property}");
                    }
                }
            }

            Console.ReadLine();
        }

        private static string GetCode()
        {
            return @"
            using System;

            namespace RuntimeAssemblyTest
            {
                public class TestClass
                {
                    public bool Property1 { get; set; }
                }

                internal struct TestStruct
                {
                    public int Property1 { get; set; }
                }
            }";
        }

        private static SyntaxNode GetSyntax()
        {
            var @namespace = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("RuntimeAssemblyTest")).NormalizeWhitespace();

            @namespace = @namespace.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")));

            var testClassDeclaration = SyntaxFactory.ClassDeclaration("TestClass")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                // public bool Property1 { get; set; }
                .AddMembers(SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName("bool"), "Property1")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddAccessorListAccessors(
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))));

            var testStructDeclaration = SyntaxFactory.StructDeclaration("TestStruct")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                // public int Property1 { get; set; }
                .AddMembers(SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName("int"), "Property1")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddAccessorListAccessors(
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))));

            @namespace = @namespace.AddMembers(testClassDeclaration, testStructDeclaration);

            return @namespace;
        }
    }
}