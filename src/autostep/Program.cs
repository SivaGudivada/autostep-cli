using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Help;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.CommandLine.Rendering;
using System.Threading.Tasks;

namespace AutoStep.CommandLine
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var parser = new CommandLineBuilder()
                            .AddCommand(new RunCommand())
                            .AddCommand(new BuildCommand())
                            .AddCommand(new NewProjectCommand())
                            .UseParseErrorReporting()
                            .CancelOnProcessTermination()
                            .UseHelp()
                            .ConfigureConsole(context =>
                            {
                                var console = context.Console;

                                var terminal = console.GetTerminal(false, OutputMode.Ansi);

                                if (terminal is object)
                                {
                                    return terminal;
                                }

                                return console;
                            })
                            .UseVersionOption()
                            .Build();

            var result = parser.Parse(args);

            if (DisplayHelp(args))
            {
                var console = new SystemConsole();
                var helpBuilder = new HelpBuilder(console);
                helpBuilder.Write(result.CommandResult.Command);
                return 0;
            }

            return await result.InvokeAsync();
        }

        private static bool DisplayHelp(string[] args) =>
            NoArgsPassed(args) ||
            NewCmdWithNoSubCmds(args);

        private static bool NoArgsPassed(string[] args) => args.Length == 0;

        private static bool NewCmdWithNoSubCmds(string[] args) => args.Length == 1 && args[0] == "new";
    }
}
