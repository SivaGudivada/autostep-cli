using System;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AutoStep.CommandLine
{
    public class NewProjectCommand : AutoStepCommand<RunArgs>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NewProjectCommand"/> class.
        /// </summary>
        public NewProjectCommand()
            : base("new", "create a new project.")
        {
            AddCommand(new CreateProjectCommand());
        }

        public override Task<int> Execute(RunArgs args, ILoggerFactory logFactory, CancellationToken cancelToken)
        {
            return Task.FromResult(1);
        }
    }

    /// <summary>
    /// A delegate to represent the project creator function
    /// </summary>
    /// <param name="args">Run args from the command line.</param>
    /// <param name="logger">logger instance.</param>
    /// <returns>success code - 0 (success) or 1 (fail).</returns>
    internal delegate int CreateProject(RunArgs args, ILogger logger);

    /// <summary>
    /// The Create project command that handles 'new project' commands passed to autostep-cli.
    /// </summary>
    internal class CreateProjectCommand : AutoStepCommand<RunArgs>
    {
        private readonly CreateProject createProj;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateProjectCommand"/> class.
        /// </summary>
        public CreateProjectCommand()
            : this("project", "creates a new project with single test file(.as) file and an empty autostep.config.json.", NewProjectFileIO.CreateBlankProjectFiles)
        {
            AddCommand(new CreateWebProjectCommand());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateProjectCommand"/> class.
        /// </summary>
        /// <param name="name">command name</param>
        /// <param name="description">command description</param>
        /// <param name="createProj">delegate reference to the method that creates the project</param>
        protected CreateProjectCommand(string name, string description, CreateProject createProj) : base(name, description)
        {
            AddCommonOptions();
            this.createProj = createProj ?? this.createProj;
        }

        private void AddCommonOptions() => AddOption(NewProjectCommandCommonOptions.SpecifyDirectoryOption);

        public override Task<int> Execute(RunArgs args, ILoggerFactory logFactory, CancellationToken cancelToken)
        {
            var logger = logFactory.CreateLogger<NewProjectCommand>();

            try
            {
                return Task.FromResult(createProj(args, logger));
            }
            catch (ProjectConfigurationException projectConfigEx)
            {
                LogConfigurationError(logger, projectConfigEx);
            }

            return Task.FromResult(1);
        }
    }

    internal class CreateWebProjectCommand : CreateProjectCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateWebProjectCommand"/> class.
        /// </summary>
        public CreateWebProjectCommand()
            : base("web", "creates a new web project with an example interactions file(.asi), tests file(.as) file and autostep.config.json with AutoStep.Web extension.", NewProjectFileIO.CreateWebProjectFiles)
        {
        }
    }

    /// <summary>
    /// Utility class that handles all File IO operations related to creating a autostep project
    /// </summary>
    internal static class NewProjectFileIO
    {
        private const string BlankWebProjectCreated = "Blank web project created.";
        private const string BlankProjectCreated = "Blank project created.";

        /// <summary>
        /// Handles creation of autostep web project.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="logger"></param>
        /// <returns>0/1 indication success/failure.</returns>
        public static int CreateWebProjectFiles(RunArgs args, ILogger logger)
        {
            var dirInfo = args.Directory;

            if (!Directory.Exists(dirInfo.FullName))
            {
                Directory.CreateDirectory(dirInfo.FullName);
            }

            void CreateAutoStepInteractionsFile()
            {
                var fpath = Path.Combine(dirInfo.FullName, dirInfo.Name + ".asi");

                // create autostep interactions file
                string asInteractionsFileContent = $"# autostep interactions file";

                CreateFileWithContent(fpath, asInteractionsFileContent);
            }

            void CreateAutoStepTestFile()
            {
                var fpath = Path.Combine(dirInfo.FullName, dirInfo.Name + ".as");

                // create autostep test file
                string asTestFileContent = $"Feature: <Feature title> {Environment.NewLine}   Scenario: Clicked on X shows Y";

                CreateFileWithContent(fpath, asTestFileContent);
            }

            void CreateAutoStepConfiguration()
            {
                var fpath = Path.Combine(dirInfo.FullName, "autostep.config.json");

                // create config file
                const string WebAutoStepConfigContent = @"{
    ""extensions"": [
      { ""package"": ""AutoStep.Web"",  ""prerelease"": true }
    ],
    ""extensionSources"": [
      ""https://f.feedz.io/autostep/ci/nuget/index.json""
    ]
} ";

                CreateFileWithContent(fpath, WebAutoStepConfigContent);
            }

            CreateAutoStepInteractionsFile();
            CreateAutoStepTestFile();
            CreateAutoStepConfiguration();

            logger.LogInformation(BlankWebProjectCreated);

            return 0;
        }

        /// <summary>
        /// Handles creation of blank autostep project.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static int CreateBlankProjectFiles(RunArgs args, ILogger logger)
        {
            var dirInfo = args.Directory;

            if (!Directory.Exists(dirInfo.FullName)) Directory.CreateDirectory(dirInfo.FullName);

            void CreateAutoStepTestFile()
            {
                var fpath = Path.Combine(dirInfo.FullName, dirInfo.Name + ".as");

                // create autostep test file
                string asTestFileContent = $"Feature: <Feature title> {Environment.NewLine}   Scenario: Clicked on X shows Y";

                CreateFileWithContent(fpath, asTestFileContent);
            }

            void CreateAutoStepConfiguration()
            {
                var fpath = Path.Combine(dirInfo.FullName, "autostep.config.json");

                // create config file
                const string BlankAutoStepConfigContent = @"{
    ""extensions"": [],
    ""extensionSources"": []
} ";

                CreateFileWithContent(fpath, BlankAutoStepConfigContent);
            }

            CreateAutoStepTestFile();
            CreateAutoStepConfiguration();

            logger.LogInformation(BlankProjectCreated);

            return 0;
        }

        private static void CreateFileWithContent(string filePath, string content)
        {
            using (var writer = File.CreateText(filePath))
            {
                writer.Write(content);
            }
        }
    }

    /// <summary>
    /// Util class to host all the common Options for creating new autostep projects.
    /// </summary>
    internal class NewProjectCommandCommonOptions
    {
        /// <summary>
        /// System.CommandLine Option for letting the user specify a directory. 
        /// Note: This option does not assert the presence of the directory specified.
        /// </summary>
        public static readonly Option SpecifyDirectoryOption = new Option(new[] { "-d", "--directory" }, "Provide the base directory for the autostep project.")
        {
            Argument = new Argument<DirectoryInfo>(() => new DirectoryInfo(Environment.CurrentDirectory)),
        };
    }
}

