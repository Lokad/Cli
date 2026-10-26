using System;
using System.Linq;
using System.Threading;
using Lokad.Cli.Generator.Analysis;
using Lokad.Cli.Generator.Emit;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lokad.Cli.Generator
{
    public abstract class BaseIncrementalGenerator : IIncrementalGenerator
    {
        private readonly string _slug;

        protected BaseIncrementalGenerator(string slug)
        {
            _slug = slug;
        }

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var allTypes = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: IsTypeDeclaration,
                transform: GetSymbolAndStrategy);

            context.RegisterSourceOutput(allTypes, GenerateSource);
        }

        /// <summary>Whether a symbol carries annotations requesting a CLI parser.</summary>
        protected abstract bool HasCliAnnotation(Compilation compilation, ITypeSymbol typeSymbol);

        /// <summary>Builds a parser strategy for an annotated symbol; declaration failures may throw generator diagnostics.</summary>
        protected abstract ITypeGenerationStrategy GetGenerationStrategyForSymbol(
            Compilation compilation,
            ITypeSymbol typeSymbol,
            CliTypeInfo type);

        private static bool IsTypeDeclaration(SyntaxNode node, CancellationToken cancel) =>
            node is TypeDeclarationSyntax declaration && declaration.AttributeLists.Count > 0;

        private GenerationItem? GetSymbolAndStrategy(
            GeneratorSyntaxContext context,
            CancellationToken cancel)
        {
            var declaration = (TypeDeclarationSyntax)context.Node;
            if (!(context.SemanticModel.GetDeclaredSymbol(declaration, cancel) is ITypeSymbol typeSymbol))
                return null;
            if (!HasCliAnnotation(context.SemanticModel.Compilation, typeSymbol))
                return null;

            // Symbols merge all partial parts. Only the first attributed part owns generation.
            foreach (var reference in typeSymbol.DeclaringSyntaxReferences)
            {
                if (!(reference.GetSyntax(cancel) is TypeDeclarationSyntax part) || part.AttributeLists.Count == 0)
                    continue;
                if (reference.SyntaxTree != declaration.SyntaxTree || reference.Span != declaration.Span)
                    return null;
                break;
            }

            var type = GetTypeInfo(typeSymbol);

            try
            {
                var strategy = GetGenerationStrategyForSymbol(context.SemanticModel.Compilation, typeSymbol, type);
                return new GenerationItem(strategy, GetPrimaryLocation(typeSymbol));
            }
            catch (Exception ex)
            {
                return new GenerationItem(new ErrorReportStrategy(ex, type), GetPrimaryLocation(typeSymbol));
            }
        }

        private void GenerateSource(SourceProductionContext context, GenerationItem? item)
        {
            if (item == null)
                return;
            var strategy = item.Strategy;

            try
            {
                ReportDiagnostics(context, item);

                var source = strategy.EmitSource();
                if (source != null)
                    context.AddSource(
                        (strategy.Type.TypeName.Namespace.Length == 0 ? "" : strategy.Type.TypeName.Namespace.Replace("@", "") + ".") +
                        $"{strategy.Type.TypeName.Name}.{_slug}.g.cs",
                        source);
            }
            catch (Exception ex)
            {
                ReportDiagnostics(context, new GenerationItem(
                    new ErrorReportStrategy(ex, strategy.Type),
                    item.PrimaryLocation));
            }
        }

        private static CliTypeInfo GetTypeInfo(ITypeSymbol typeSymbol)
        {
            return new CliTypeInfo(
                new TypeName(
                    typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    typeSymbol.Name,
                    typeSymbol.ContainingNamespace.IsGlobalNamespace ? "" : typeSymbol.ContainingNamespace.ToDisplayString()),
                GetTypeKind(typeSymbol));
        }

        private static CliTypeKind GetTypeKind(ITypeSymbol typeSymbol)
        {
            if (typeSymbol.TypeKind == TypeKind.Interface)
                return CliTypeKind.Interface;

            if (typeSymbol.IsRecord && typeSymbol.TypeKind == TypeKind.Struct)
                return CliTypeKind.RecordStruct;

            if (typeSymbol.IsRecord)
                return CliTypeKind.Record;

            if (typeSymbol.TypeKind == TypeKind.Struct)
                return CliTypeKind.Struct;

            return CliTypeKind.Class;
        }

        private static Location? GetPrimaryLocation(ITypeSymbol typeSymbol) =>
            typeSymbol.Locations.FirstOrDefault(location => location.IsInSource);

        private static void ReportDiagnostics(SourceProductionContext context, GenerationItem item)
        {
            foreach (var diagnostic in item.Strategy.Diagnostics)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    diagnostic.Descriptor,
                    diagnostic.Location ?? item.PrimaryLocation,
                    diagnostic.MessageArgs));
            }
        }

        private sealed class GenerationItem
        {
            public GenerationItem(ITypeGenerationStrategy strategy, Location? primaryLocation)
            {
                Strategy = strategy;
                PrimaryLocation = primaryLocation;
            }

            public ITypeGenerationStrategy Strategy { get; }

            public Location? PrimaryLocation { get; }
        }
    }
}
