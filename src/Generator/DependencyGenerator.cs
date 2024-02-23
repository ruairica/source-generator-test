using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;

namespace Generator;

[Generator]
public class DependencyGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (syntaxNode, _) => syntaxNode is ClassDeclarationSyntax,
                transform: static (ctx, _) => (ClassDeclarationSyntax)ctx.Node)
            .Where(syntax => syntax is not null);

        var compilation = context.CompilationProvider.Combine(provider.Collect());

        context.RegisterSourceOutput(compilation, Execute);
    }

    private static void Execute(
        SourceProductionContext context,
        (Compilation Left, ImmutableArray<ClassDeclarationSyntax> Right) arg2)
    {
        var (compilation, list) = arg2;

        var namespaces = new HashSet<string>();
        var interfaceImplementationMap = new Dictionary<string, HashSet<string>>();
        var interfacesThatAreInjected = new HashSet<string>();

        // TODO fix: currently adds excessive namespaces
        foreach (var symbol in list.Select(
                     syntax => compilation.GetSemanticModel(syntax.SyntaxTree)
                         .GetDeclaredSymbol(syntax)))
        {
            if (symbol.Interfaces.Length > 0)
            {
                namespaces.Add(symbol.ContainingNamespace.ToDisplayString());
                foreach (var i in symbol.Interfaces)
                {
                    interfaceImplementationMap.AddOrAppend(i.Name, symbol.Name);
                }
            }

            if (!symbol.Constructors.Any())
            {
                continue;
            }

            foreach (var item in symbol.Constructors.SelectMany(x => x.Parameters)
                         .Where(x => x.Type.TypeKind == TypeKind.Interface)
                         .Select(x => x.Type))
            {
                if (item.Name == "IEnumerable")
                {
                    // handle a class expects IEnumerable ofDependencies
                    var ienumerableSymbol = (item as INamedTypeSymbol);

                    if (ienumerableSymbol.TypeArguments.First().TypeKind ==
                        TypeKind.Interface)
                    {
                        namespaces.Add(item.ContainingNamespace.ToDisplayString());

                        interfacesThatAreInjected.Add(
                            ienumerableSymbol.TypeArguments.First().Name);

                        continue;
                    }
                }

                namespaces.Add(item.ContainingNamespace.ToDisplayString());
                interfacesThatAreInjected.Add(item.Name);
            }
        }

        var sb = new StringBuilder();
        sb.AppendLine("namespace DependencyGen");
        sb.AppendLine("{");
        sb.AppendLine(string.Join("\n", namespaces.Select(n => $"   using {n};")));
        sb.AppendLine("    public static class DependencyGenerator");
        sb.AppendLine("    {");

        sb.AppendLine(
            "        public static IServiceCollection AddGeneratedDependencies(this IServiceCollection services)");

        sb.AppendLine("        {");

        foreach (var intf in interfacesThatAreInjected)
        {
            if (interfaceImplementationMap.TryGetValue(intf, out var implementations))
            {
                sb.AppendLine(
                    string.Join(
                        "\n",
                        implementations.Select(
                            implementation =>
                                $"            services.AddTransient<{intf}, {implementation}>();")));
            }
        }

        sb.AppendLine("            return services;");
        sb.AppendLine("        }");
        sb.AppendLine("    }");

        sb.AppendLine("}");

        context.AddSource("DependencyGenerator.g.cs", sb.ToString());
    }
}