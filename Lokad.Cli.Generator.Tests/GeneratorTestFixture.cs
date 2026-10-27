using System.Globalization;
using System.Reflection;
using System.Text;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

#pragma warning disable RS1035

namespace Lokad.Cli.Generator.Tests;

public static class GeneratorTestFixture
{
    private static readonly string TestsRoot;
    private static readonly CSharpParseOptions ParseOptions = new(LanguageVersion.CSharp11);

    static GeneratorTestFixture()
    {
        var assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if (!TryFindTestsRoot(assemblyDirectory, out var baseDirectoryRoot))
            throw new DirectoryNotFoundException("Could not locate Tests folder from executable directory.");

        TestsRoot = baseDirectoryRoot;
    }

    private static IReadOnlyList<(string FileName, string Source)> Generate(string testsRoot, IReadOnlyList<string> inputFiles, string assemblyName)
    {
        var syntaxTrees = inputFiles
            .Select(relativePath =>
            {
                var source = File.ReadAllText(Path.Combine(testsRoot, relativePath));
                return CSharpSyntaxTree.ParseText(source, ParseOptions, path: relativePath);
            })
            .ToArray();

        return Generate(syntaxTrees, assemblyName, new LokadCliGenerator(), Array.Empty<MetadataReference>());
    }

    internal static void AssertCompiles(string source) => AssertCompiles(source, new LokadCliGenerator());

    internal static void AssertCompiles(string source, IIncrementalGenerator generator) =>
        AssertCompiles(source, generator, Array.Empty<MetadataReference>());

    internal static void AssertCompiles(string source, IIncrementalGenerator generator, IReadOnlyList<MetadataReference> additionalReferences)
    {
        var generated = Generate(new[] { CSharpSyntaxTree.ParseText(source, ParseOptions, path: "Example.cs") }, "Example", generator, additionalReferences);
        Assert.DoesNotContain(generated, file => file.FileName == "Diagnostics.txt");
    }

    private static IReadOnlyList<(string FileName, string Source)> Generate(
        IReadOnlyList<SyntaxTree> syntaxTrees, string assemblyName, IIncrementalGenerator generator, IReadOnlyList<MetadataReference> additionalReferences)
    {

        var runtimeDirectory = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory();
        var references = new[]
        {
            MetadataReference.CreateFromFile(Path.Combine(runtimeDirectory, "System.Runtime.dll")),
            MetadataReference.CreateFromFile(Path.Combine(runtimeDirectory, "System.Private.CoreLib.dll")),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ArgumentsAttribute).Assembly.Location).WithAliases(new[] { "global", "runtime" }),
        };

        var compilation = CSharpCompilation.Create(
            assemblyName,
            syntaxTrees,
            references.Concat(additionalReferences),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var syntaxDiagnostics = syntaxTrees.SelectMany(tree => tree.GetDiagnostics())
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
        var parseDiagnostics = compilation.GetParseDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            new[] { generator.AsSourceGenerator() },
            parseOptions: ParseOptions);

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);
        return CollectResults(driver, outputCompilation, diagnostics, syntaxDiagnostics, parseDiagnostics);
    }

    private static IReadOnlyList<(string FileName, string Source)> CollectResults(
        GeneratorDriver driver,
        Compilation outputCompilation,
        ImmutableArray<Diagnostic> diagnostics,
        IEnumerable<Diagnostic> syntaxDiagnostics,
        IEnumerable<Diagnostic> parseDiagnostics)
    {
        var result = driver.GetRunResult();
        foreach (var run in result.Results)
            Assert.True(run.Exception == null, $"Generator failed: {run.Exception}");

        var failures = diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error && !IsGeneratorDiagnostic(diagnostic));
        Assert.True(!failures.Any(), string.Join(Environment.NewLine, failures));
        var generatorDiagnostics = result.Results
            .SelectMany(runResult => runResult.Diagnostics)
            .Where(IsGeneratorDiagnostic);
        var driverDiagnostics = diagnostics.Where(IsGeneratorDiagnostic);
        var outputDiagnostics = outputCompilation.GetDiagnostics().Where(IsGeneratorDiagnostic);
        var resultDiagnostics = result.Diagnostics.Where(IsGeneratorDiagnostic);
        var allDiagnostics = syntaxDiagnostics
            .Concat(parseDiagnostics)
            .Concat(driverDiagnostics)
            .Concat(generatorDiagnostics)
            .Concat(outputDiagnostics)
            .Concat(resultDiagnostics)
            .GroupBy(diagnostic => new
            {
                diagnostic.Id,
                diagnostic.Severity,
                Message = diagnostic.GetMessage(CultureInfo.InvariantCulture),
                Location = FormatLocation(diagnostic.Location)
            })
            .Select(group => group.First())
            .ToArray();

        // Invalid declarations may leave dependent types without generated members.
        // Successful generation must produce a complete C# 11 assembly.
        if (!allDiagnostics.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error && IsGeneratorDiagnostic(diagnostic)))
        {
            using var stream = new MemoryStream();
            var emitted = outputCompilation.Emit(stream);
            Assert.True(emitted.Success, string.Join(Environment.NewLine, emitted.Diagnostics));
        }

        var generatedSources =
            (from generatorResult in result.Results
             from generated in generatorResult.GeneratedSources
             select (generated.HintName, generated.SourceText.ToString())).ToList();

        if (allDiagnostics.Length > 0)
            generatedSources.Add(("Diagnostics.txt", FormatDiagnostics(allDiagnostics)));

        return generatedSources;
    }

    public static bool TryRefresh(string folderName) => TryRefreshFixture(TestsRoot, folderName);

    internal static bool TryRefreshFixture(string testsRoot, string folderName)
    {
        // Fixture directory names also become generated test method names.
        if (!SyntaxFacts.IsValidIdentifier(folderName))
            return false;
        var outputDirectory = Path.Combine(testsRoot, folderName);
        var directory = new DirectoryInfo(outputDirectory);
        if (!directory.Exists || (directory.Attributes & FileAttributes.ReparsePoint) != 0)
            return false;
        var inputFiles = GetInputFiles(testsRoot, folderName);
        if (inputFiles.Count == 0)
            return false;

        var generatedSources = Generate(testsRoot, inputFiles, folderName);

        foreach (var file in Directory.EnumerateFiles(outputDirectory))
        {
            var fileName = Path.GetFileName(file);
            if (IsSnapshotFile(fileName))
                File.Delete(file);
        }

        foreach (var (hintName, source) in generatedSources)
        {
            File.WriteAllText(Path.Combine(outputDirectory, hintName), source);
        }
        return true;
    }

    public static void Test(string folderName)
    {
        var generatedSources = Generate(TestsRoot, GetInputFiles(TestsRoot, folderName), folderName);
        var outputDirectory = Path.Combine(TestsRoot, folderName);

        var inputPath = Path.Combine(TestsRoot, folderName, "Input.cs");
        var inputBytes = new FileInfo(inputPath).Length;
        var outputFiles = generatedSources.Count;
        var outputBytes = generatedSources.Sum(source => source.Source.Length);
        Console.WriteLine($"{folderName}: {inputBytes} input bytes, {outputFiles} output files, {outputBytes} output bytes");

        var expectedFiles = Directory.Exists(outputDirectory)
            ? Directory.EnumerateFiles(outputDirectory)
                .Select(Path.GetFileName)
                .OfType<string>()
                .Where(IsSnapshotFile)
                .ToArray()
            : Array.Empty<string>();

        var generatedFiles = generatedSources.Select(source => source.FileName).ToArray();

        Array.Sort(expectedFiles, StringComparer.Ordinal);
        Array.Sort(generatedFiles, StringComparer.Ordinal);
        Assert.Equal(expectedFiles, generatedFiles);

        foreach (var (hintName, source) in generatedSources)
        {
            var expectedSource = LoadContent(Path.Combine(folderName, hintName));
            Assert.Equal(expectedSource, source);
        }
    }

    public static void Reindex()
    {
        var testNames = GetTestDirectoryNames();
        var projectRoot = Directory.GetParent(TestsRoot)?.FullName
            ?? throw new DirectoryNotFoundException("Could not locate project root for GeneratorTests.cs.");
        var testsFilePath = Path.Combine(projectRoot, "GeneratorTests.cs");
        var content = File.ReadAllText(testsFilePath);
        var updated = ReplaceAutoGeneratedRegion(content, testNames);
        File.WriteAllText(testsFilePath, updated);
    }

    public static void RefreshAll()
    {
        var testNames = GetTestDirectoryNames();
        foreach (var testName in testNames)
        {
            if (!TryRefresh(testName))
                throw new DirectoryNotFoundException($"No input files found for fixture '{testName}'.");
        }
    }

    private static string FormatDiagnostics(IReadOnlyList<Diagnostic> diagnostics)
    {
        if (diagnostics.Count == 0)
            return "None";

        var ordered = diagnostics
            .OrderBy(diagnostic => diagnostic.Severity)
            .ThenBy(diagnostic => diagnostic.Id, StringComparer.Ordinal)
            .ThenBy(diagnostic => diagnostic.Location.IsInSource ? diagnostic.Location.SourceSpan.Start : int.MaxValue)
            .ThenBy(diagnostic => diagnostic.GetMessage(CultureInfo.InvariantCulture), StringComparer.Ordinal)
            .ToArray();

        var builder = new StringBuilder();
        foreach (var diagnostic in ordered)
        {
            var location = FormatLocation(diagnostic.Location);
            var message = diagnostic.GetMessage(CultureInfo.InvariantCulture);
            builder.Append(diagnostic.Severity)
                .Append(' ')
                .Append(diagnostic.Id)
                .Append(": ")
                .Append(message)
                .Append(" (")
                .Append(location)
                .Append(')')
                .AppendLine();
        }

        return builder.ToString().TrimEnd();
    }

    private static string FormatLocation(Location location)
    {
        if (location == Location.None)
            return "NoLocation";

        if (!location.IsInSource)
            return "NoSource";

        var lineSpan = location.GetLineSpan();
        var path = string.IsNullOrWhiteSpace(lineSpan.Path) ? "Unknown" : lineSpan.Path.Replace('\\', '/');
        var line = lineSpan.StartLinePosition.Line + 1;
        var column = lineSpan.StartLinePosition.Character + 1;
        return $"{path}:{line}:{column}";
    }

    private static string LoadContent(string relativePath)
    {
        var path = Path.Combine(TestsRoot, relativePath);
        return File.ReadAllText(path);
    }

    private static IEnumerable<string> GetTestDirectoryNames()
    {
        var testsDirectory = new DirectoryInfo(TestsRoot);
        return testsDirectory.EnumerateDirectories()
            .Where(directory => File.Exists(Path.Combine(directory.FullName, "Input.cs")))
            .Select(directory => directory.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
    }

    private static IReadOnlyList<string> GetInputFiles(string testsRoot, string folderName)
    {
        var folderPath = Path.Combine(testsRoot, folderName);
        if (!Directory.Exists(folderPath))
            return Array.Empty<string>();

        return Directory.EnumerateFiles(folderPath, "*.cs")
            .Select(Path.GetFileName)
            .OfType<string>()
            .Where(IsInputFile)
            .Select(fileName => Path.Combine(folderName, fileName))
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();
    }

    private static bool IsInputFile(string? fileName) =>
        fileName != null &&
        fileName.StartsWith("Input", StringComparison.OrdinalIgnoreCase) &&
        fileName.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) &&
        !fileName.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase);

    private static bool IsSnapshotFile(string fileName) =>
        fileName.EndsWith(".lkc.g.cs", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(fileName, "Diagnostics.txt", StringComparison.OrdinalIgnoreCase);

    private static bool IsGeneratorDiagnostic(Diagnostic diagnostic) =>
        diagnostic.Id.StartsWith("LKC", StringComparison.Ordinal);

    private static string ReplaceAutoGeneratedRegion(string content, IEnumerable<string> testNames)
    {
        const string regionMarker = "#region AUTO-GENERATED CODE";
        const string endRegionMarker = "#endregion";

        var regionIndex = content.IndexOf(regionMarker, StringComparison.Ordinal);
        if (regionIndex < 0)
            throw new InvalidOperationException("Could not find auto-generated region marker.");

        var endRegionIndex = content.IndexOf(endRegionMarker, regionIndex, StringComparison.Ordinal);
        if (endRegionIndex < 0)
            throw new InvalidOperationException("Could not find end of auto-generated region.");

        var regionLineStart = content.LastIndexOf('\n', regionIndex);
        regionLineStart = regionLineStart < 0 ? 0 : regionLineStart + 1;
        var regionLineEnd = content.IndexOf('\n', regionIndex);
        if (regionLineEnd < 0)
            regionLineEnd = content.Length;

        var endRegionLineStart = content.LastIndexOf('\n', endRegionIndex);
        endRegionLineStart = endRegionLineStart < 0 ? 0 : endRegionLineStart + 1;

        var indent = content.Substring(regionLineStart, regionIndex - regionLineStart);
        var testLines = testNames.Select(name => $"{indent}[Fact] public void {name}() => GeneratorTestFixture.Test(nameof({name}));");
        var newBody = $"\n{string.Join("\n", testLines)}\n\n";

        return string.Concat(
            content.AsSpan(0, regionLineEnd + 1),
            newBody,
            content.AsSpan(endRegionLineStart));
    }

    private static bool TryFindTestsRoot(string? startPath, out string testsRoot)
    {
        var current = startPath is null ? null : new DirectoryInfo(startPath);
        while (current != null)
        {
            var candidate = Path.Combine(current.FullName, "Tests");
            if (Directory.Exists(candidate))
            {
                testsRoot = candidate;
                return true;
            }

            current = current.Parent;
        }

        testsRoot = string.Empty;
        return false;
    }
}
