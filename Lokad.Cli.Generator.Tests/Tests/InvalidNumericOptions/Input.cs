using Lokad.Cli;

namespace Test;

[Arguments("Duplicate numeric shorthand.")]
public partial record DuplicateNumeric(
    [property: Argument("First.", ShortNumeric = true)] int? First,
    [property: Argument("Second.", ShortNumeric = true)] int? Second);

[Arguments("Digit flag.")]
public partial record DigitFlag(
    [property: Argument("One.", Short = '1')] bool One,
    [property: Argument("Number.", ShortNumeric = true)] int? Number);

[Arguments("Digit alias.")]
public partial record DigitAlias(
    [property: Argument("Number.", ShortNumeric = true, ShortNames = new[] { 'n', '2' })] int? Number);

[Arguments("Digit discriminator.", Short = '3')]
public partial record DigitDiscriminator(
    [property: Argument("Number.", ShortNumeric = true)] int? Number);
