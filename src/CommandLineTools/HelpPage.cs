using MaSch.Console;
using MaSch.Console.Cli;
using MaSch.Console.Cli.Runtime;
using System.Collections.Generic;
using System.Linq;

namespace MaSch.CommandLineTools;

public class HelpPage : CliHelpPage
{
    private readonly IEnumerable<IHelpPageMutator> _mutators;

    public HelpPage(ICliApplicationBase application, IConsoleService console, IEnumerable<IHelpPageMutator> mutators)
        : base(application, console)
    {
        _mutators = mutators;
    }

    protected override IEnumerable<ICliCommandOptionInfo> OrderOptions(CliError error, IEnumerable<ICliCommandOptionInfo> options)
    {
        var mutator = _mutators.OfType<IOptionOrderMutator>().FirstOrDefault(x => x.CanMutate(error));
        if (mutator != null)
            return mutator.OrderOptions(error, options);

        return base.OrderOptions(error, options);
    }
}

public interface IHelpPageMutator
{
    bool CanMutate(CliError error);
}

public interface IOptionOrderMutator : IHelpPageMutator
{
    IEnumerable<ICliCommandOptionInfo> OrderOptions(CliError error, IEnumerable<ICliCommandOptionInfo> options);
}
