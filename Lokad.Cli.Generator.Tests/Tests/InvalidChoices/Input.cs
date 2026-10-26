using Lokad.Cli;

namespace Test;

[Choice(typeof(ISelf))]
public partial interface ISelf { }

[Choice(typeof(FirstDefault))]
[Choice(typeof(SecondDefault))]
public partial interface IMultipleDefaults { }

[Arguments("First default.")]
public partial record FirstDefault : IMultipleDefaults, IMixedNullChoice;

[Arguments("Second default.")]
public partial record SecondDefault : IMultipleDefaults;

[Choice(typeof(FirstRun))]
[Choice(typeof(SecondRun))]
public partial interface IDuplicateSelectors { }

[Arguments("First run.", Prefix = "run")]
public partial record FirstRun : IDuplicateSelectors;

[Arguments("Second run.", Prefix = "run")]
public partial record SecondRun : IDuplicateSelectors;

[Choice(typeof(Unrelated))]
public partial interface IUnrelatedChoice { }

public class Unrelated { }

[Choice(typeof(Unannotated))]
public partial interface IUnannotatedChoice { }

public class Unannotated : IUnannotatedChoice { }

[Choice(null)]
public partial interface INullChoice { }

[Choice(typeof(FirstDefault))]
[Choice(null)]
public partial interface IMixedNullChoice { }
