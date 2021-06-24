using MaSch.Console.Cli;
using MaSch.Console.Cli.Runtime;
using System.Collections.Generic;
using System.Linq;

namespace MaSch.CommandLineTools
{
    public class HelpPage : CliHelpPage
    {
        private readonly List<IHelpPageMutator> _mutators = new();

        public HelpPage WithMutator(IHelpPageMutator mutator)
        {
            _mutators.Add(mutator);
            return this;
        }

        protected override IEnumerable<ICliCommandOptionInfo> OrderOptions(ICliApplicationBase application, CliError error, IEnumerable<ICliCommandOptionInfo> options)
        {
            var mutator = _mutators.OfType<IOptionOrderMutator>().FirstOrDefault(x => x.CanMutate(application, error));
            if (mutator != null)
                return mutator.OrderOptions(application, error, options);

            return base.OrderOptions(application, error, options);
        }
    }

    public interface IHelpPageMutator
    {
        bool CanMutate(ICliApplicationBase application, CliError error);
    }

    public interface IOptionOrderMutator : IHelpPageMutator
    {
        IEnumerable<ICliCommandOptionInfo> OrderOptions(ICliApplicationBase application, CliError error, IEnumerable<ICliCommandOptionInfo> options);
    }
}
