﻿using System;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoStep.Extensions;
using AutoStep.Extensions.Abstractions;
using AutoStep.Language;
using AutoStep.Language.Interaction;
using AutoStep.Language.Test;
using AutoStep.Projects;
using AutoStep.Projects.Files;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AutoStep.CommandLine
{
    public abstract class BuildOperationCommand<TArgs> : AutoStepCommand<TArgs>
        where TArgs : BuildOperationArgs
    {
        public BuildOperationCommand(string name, string description = null) : base(name, description)
        {
            Add(new Option(new[] { "-d", "--directory" }, "Provide the base directory for the autostep project.")
            {
                Argument = new Argument<DirectoryInfo>(() => new DirectoryInfo(Environment.CurrentDirectory)).ExistingOnly()
            });
        }

        protected IConfiguration GetConfiguration(BaseProjectArgs args)
        {
            var configurationBuilder = new ConfigurationBuilder();

            var configFile = args.Config;

            if(configFile is null)
            {
                configFile = new FileInfo(Path.Combine(args.Directory.FullName, "autostep.config.json"));
            }

            // Is there a config file?
            if (configFile.Exists)
            {
                // Add the JSON file.
                configurationBuilder.AddJsonFile(configFile.FullName);
            }

            // Add environment.
            configurationBuilder.AddEnvironmentVariables("AutoStep");

            // Add the provided command line options.
            configurationBuilder.AddInMemoryCollection(args.Option);

            return configurationBuilder.Build();
        }

        protected Project CreateProject(BaseProjectArgs args, IConfiguration projectConfig, ILoadedExtensions<IExtensionEntryPoint> extensions)
        {
            // Create the project.
            Project project;

            if (args.Diagnostic)
            {
                project = new Project(p => ProjectCompiler.CreateWithOptions(p, TestCompilerOptions.EnableDiagnostics, InteractionsCompilerOptions.EnableDiagnostics, false));
            }
            else
            {
                project = new Project();
            }

            // Let our extensions extend the project.
            foreach (var ext in extensions.ExtensionEntryPoints)
            {
                ext.AttachToProject(projectConfig, project);
            }

            // Add any files from extension content.
            // Treat the extension directory as a single file set (one for interactions, one for test).
            var extInteractionFiles = FileSet.Create(extensions.ExtensionsRootDir, new string[] { "*/content/**/*.asi" });
            var extTestFiles = FileSet.Create(extensions.ExtensionsRootDir, new string[] { "*/content/**/*.as" });

            project.MergeInteractionFileSet(extInteractionFiles);
            project.MergeTestFileSet(extTestFiles);

            // Define file sets for interaction and test.
            var interactionFiles = FileSet.Create(args.Directory.FullName, projectConfig.GetInteractionFileGlobs(), new string[] { ".autostep/**" });
            var testFiles = FileSet.Create(args.Directory.FullName, projectConfig.GetTestFileGlobs(), new string[] { ".autostep/**" });

            // Add the two file sets.
            project.MergeInteractionFileSet(interactionFiles);
            project.MergeTestFileSet(testFiles);

            return project;
        }

        protected async Task<bool> BuildAndWriteResultsAsync(BuildOperationArgs args, Project project, ILoggerFactory logFactory, CancellationToken cancelToken)
        {
            var logger = logFactory.CreateLogger("build");

            logger.LogInformation("Compiling Project.");

            // Execution.
            var compiled = await CompileAsync(args, project, logFactory, cancelToken);

            var success = true;

            // Write results.
            WriteBuildResults(logFactory, compiled);

            if (compiled.Messages.Any(m => m.Level == CompilerMessageLevel.Error))
            {
                logger.LogWarning("Compilation failed with one or more errors.");
                success = false;
            }
            else
            {
                logger.LogInformation("Compiled successfully.");
            }

            logger.LogInformation("Binding Steps.");

            var linked = Link(args, project, logFactory, cancelToken);

            if (success && linked.Messages.Any(m => m.Level == CompilerMessageLevel.Error))
            {
                logger.LogWarning("Step binding failed with one or more errors.");
                success = false;
            }
            else
            {
                logger.LogInformation("All steps bound successfully.");
            }

            // Write link result.
            WriteBuildResults(logFactory, linked);

            return success;
        }

        protected void WriteBuildResults(ILoggerFactory logFactory, ProjectCompilerResult result)
        {
            var logger = logFactory.CreateLogger<Program>();

            foreach (var message in result.Messages)
            {
                var logLevel = message.Level switch
                {
                    CompilerMessageLevel.Error => LogLevel.Error,
                    _ => LogLevel.Information
                };

                logger.Log(logLevel, message.ToString());
            }
        }

        protected async Task<ILoadedExtensions<IExtensionEntryPoint>> LoadExtensionsAsync(BaseProjectArgs projectArgs, ILoggerFactory logFactory, IConfiguration projectConfig, CancellationToken cancelToken)
        {
            var sourceSettings = new SourceSettings(projectArgs.Directory.FullName);

            var customSources = projectConfig.GetSection("extensionSources").Get<string[]>() ?? Array.Empty<string>();

            if (customSources.Length > 0)
            {
                // Add any additional configured sources.
                sourceSettings.AppendCustomSources(customSources);
            }

            var extensionsDir = Path.Combine(projectArgs.Directory.FullName, ".autostep", "extensions");
            var setLoader = new ExtensionSetLoader(extensionsDir, logFactory, "autostep");

            var loaded = await setLoader.LoadExtensionsAsync<IExtensionEntryPoint>(sourceSettings, projectConfig.GetExtensionConfiguration(), false, cancelToken);

            return loaded;
        }

        protected async ValueTask<ProjectCompilerResult> CompileAsync(BuildOperationArgs args, Project project, ILoggerFactory logFactory, CancellationToken cancelToken)
        {
            // Now, compile.
            return await project.Compiler.CompileAsync(logFactory, cancelToken);
        }

        protected ProjectCompilerResult Link(BuildOperationArgs args, Project project, ILoggerFactory logFactory, CancellationToken cancelToken)
        {
            return project.Compiler.Link(cancelToken);
        }
    }
}
