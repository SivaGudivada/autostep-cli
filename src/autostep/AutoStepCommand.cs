﻿using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AutoStep.CommandLine
{
    /// <summary>
    /// Base class for all autostep CLI commands.
    /// </summary>
    /// <typeparam name="TArgs">The argument type (that is bound to the command line options).</typeparam>
    internal abstract class AutoStepCommand<TArgs> : Command
        where TArgs : BaseGlobalArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AutoStepCommand{TArgs}"/> class.
        /// </summary>
        /// <param name="name">The name of the command.</param>
        /// <param name="description">The command description.</param>
        public AutoStepCommand(string name, string? description = null)
            : base(name, description)
        {
            AddCommonOptions();

            Handler = CommandHandler.Create(async (TArgs args, IConsole con, CancellationToken token) =>
            {
                using var logFactory = GetLoggerFactory(args, con);
                return await Execute(args, GetLoggerFactory(args, con), token);
            });
        }

        /// <summary>
        /// Get a logger factory instance backed by the console.
        /// </summary>
        /// <param name="args">The provided arguments.</param>
        /// <param name="console">The console instance.</param>
        /// <returns>A logger factory.</returns>
        protected ILoggerFactory GetLoggerFactory(BaseGlobalArgs args, IConsole console)
        {
            return LoggerFactory.Create(cfg =>
            {
                var minLevel = LogLevel.Information;

                if (args.Verbose)
                {
                    minLevel = LogLevel.Debug;
                }

                if (args.Diagnostic)
                {
                    minLevel = LogLevel.Trace;
                }

                cfg.AddProvider(new ConsoleLoggerProvider(console, minLevel));
                cfg.SetMinimumLevel(minLevel);
            });
        }

        /// <summary>
        /// Execute the command.
        /// </summary>
        /// <param name="args">The bound command-line arguments.</param>
        /// <param name="logFactory">A logger factory.</param>
        /// <param name="cancelToken">A cancellation token.</param>
        /// <returns>Returns an awaitable object that outputs the command's exit code.</returns>
        public abstract Task<int> Execute(TArgs args, ILoggerFactory logFactory, CancellationToken cancelToken);

        /// <summary>
        /// Log a configuration error.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="configEx">The configuration exception.</param>
        protected void LogConfigurationError(ILogger logger, ProjectConfigurationException configEx)
        {
            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (configEx is null)
            {
                throw new ArgumentNullException(nameof(configEx));
            }

            logger.LogError(Messages.ProjectConfigurationError, configEx.Message);
        }

        private void AddCommonOptions()
        {
            Add(new Option(new[] { "-d", "--directory" }, "Provide the base directory for the autostep project.")
            {
                Argument = new Argument<DirectoryInfo>(() => new DirectoryInfo(Environment.CurrentDirectory)).ExistingOnly(),
            });

            Add(new Option(new[] { "-c", "--config" }, "Specify the autostep configuration file.")
            {
                Argument = new Argument<FileInfo?>(() => null).ExistingOnly(),
            });

            Add(new Option(new[] { "-o", "--option" }, "Specify key/value pairs to override project configuration settings.")
            {
                Argument = new Argument<IReadOnlyList<KeyValuePair<string, string>>>(ParseKeyValuePairs),
            });

            Add(new Option(new[] { "-v", "--verbose" }, "Enable verbose execution, providing more execution detail."));

            Add(new Option(new[] { "--diagnostic" }, "Enables diagnostic verbosity level, providing internal execution details."));
        }

        private IReadOnlyList<KeyValuePair<string, string>> ParseKeyValuePairs(ArgumentResult parsed)
        {
            var list = new List<KeyValuePair<string, string>>();

            foreach (var item in parsed.Tokens)
            {
                if (item.Type == TokenType.Argument)
                {
                    // Split in two on the equals sign. If only 1 side, then assume boolean true.
                    var splitValue = item.Value.Split('=', 2);

                    var key = splitValue[0];

                    if (string.IsNullOrEmpty(key))
                    {
                        // Can't have an empty key.
                        parsed.ErrorMessage = string.Format(CultureInfo.CurrentCulture, Messages.EmptyKeyValueForOptionArgument, item.Value);
                        continue;
                    }

                    if (splitValue.Length > 1)
                    {
                        // Providing a value.
                        list.Add(new KeyValuePair<string, string>(key, splitValue[1]));
                    }
                    else
                    {
                        // Boolean true.
                        list.Add(new KeyValuePair<string, string>(key, true.ToString(CultureInfo.InvariantCulture)));
                    }
                }
            }

            return list;
        }
    }
}
