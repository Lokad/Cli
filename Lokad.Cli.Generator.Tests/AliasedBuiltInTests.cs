using Microsoft.CodeAnalysis;

namespace Lokad.Cli.Generator.Tests;

public sealed class AliasedBuiltInTests
{
    [Theory]
    [InlineData(false, "fr-FR")]
    [InlineData(true, "fr-FR")]
    [InlineData(false, "en-US")]
    [InlineData(true, "en-US")]
    public void PreservesInvariantDateConversionWithFrameworkAliases(bool aliased, string culture)
    {
        var references = aliased
            ? new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location).WithAliases(new[] { "global", "framework" }) }
            : Array.Empty<MetadataReference>();
        const string source = """
            using System;
            using System.Globalization;
            using Lokad.Cli;
            namespace Example;
            [Arguments("Group.")]
            public partial record Group([property: Argument("Group date.", Long = "group")] DateTime Date);
            [Arguments("Dates.")]
            public partial record Args(
                [property: Argument("Date.", Long = "date")] DateTime Date,
                [property: Argument("Optional date.", Long = "optional")] DateTime? Optional,
                [property: Argument("Dates.", Long = "many")] DateTime[] Many,
                [property: Argument("Format.", Long = "format")] OptionalValue<DateTime> Format,
                [property: Argument("Group.")] Group Group,
                [property: Argument("Number.", Long = "number")] int Number);
            public static class Usage
            {
                public static string Run()
                {
                    var saved = CultureInfo.CurrentCulture;
                    try
                    {
                        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("CULTURE");
                        var parsed = Parser.Parse<Args>(new[] { "--date=01/02/2024", "--optional=01/02/2024",
                            "--many=01/02/2024", "--many=01/03/2024", "--format=01/02/2024", "--group=01/02/2024", "--number=7" });
                        return string.Join("|", new[] { Format(parsed.Date), Format(parsed.Optional ?? throw new Exception("Missing date.")),
                            Format(parsed.Many[0]), Format(parsed.Many[1]), Format(parsed.Format.Value), Format(parsed.Group.Date),
                            parsed.Number.ToString(CultureInfo.InvariantCulture) });
                    }
                    finally { CultureInfo.CurrentCulture = saved; }
                }
                private static string Format(DateTime date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
            """;
        GeneratorTestFixture.AssertRuns(source.Replace("CULTURE", culture), references,
            "2024-01-02|2024-01-02|2024-01-02|2024-01-03|2024-01-02|2024-01-02|7");
    }
}
