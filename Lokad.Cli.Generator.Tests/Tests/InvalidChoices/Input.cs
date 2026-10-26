using Lokad.Cli;

namespace Test;

[Choice(typeof(ISelf))]
public partial interface ISelf { }

[Choice(typeof(FirstDefault))]
[Choice(typeof(SecondDefault))]
public partial interface IMultipleDefaults { }

[Arguments("First default.")]
public partial record FirstDefault : IMultipleDefaults;

[Arguments("Second default.")]
public partial record SecondDefault : IMultipleDefaults;

[Choice(typeof(FirstRun))]
[Choice(typeof(SecondRun))]
public partial interface IDuplicateSelectors { }

[Arguments("First run.", Prefix = "run")]
public partial record FirstRun : IDuplicateSelectors;

[Arguments("Second run.", Prefix = "run")]
public partial record SecondRun : IDuplicateSelectors;
