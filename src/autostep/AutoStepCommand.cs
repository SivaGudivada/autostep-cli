﻿using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
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
    public abstract class AutoStepCommand<TArgs> : Command
        where TArgs : BaseGlobalArgs
    {
        public AutoStepCommand(string name, string description = null) : base(name, description)
        {
            AddCommonOptions();

            Handler = CommandHandler.Create(async (TArgs args, IConsole con, CancellationToken token) => {
                using var logFactory = GetLoggerFactory(args, con);
                return await Execute(args, GetLoggerFactory(args, con), token);
            });
        }

        protected ILoggerFactory GetLoggerFactory(BaseGlobalArgs args, IConsole console)
        {
            return LoggerFactory.Create(cfg =>
            {
                cfg.AddProvider(new ConsoleLoggerProvider(console));

                var minLevel = LogLevel.Information;
                
                if(args.Verbose)
                {
                    minLevel = LogLevel.Debug;
                }

                if(args.Diagnostic)
                {
                    minLevel = LogLevel.Trace;
                }

                cfg.SetMinimumLevel(minLevel);
            });
        }

        public abstract Task<int> Execute(TArgs args, ILoggerFactory logFactory, CancellationToken cancelToken);

        protected void LogConfigurationError(ILogger logger, ProjectConfigurationException configEx)
        {
            logger.LogError("Project Configuration Error: {0}", configEx.Message);
        }

        private void AddCommonOptions()
        {
            Add(new Option(new[] { "-c", "--config" }, "Specify the autostep configuration file.")
            {
                Argument = new Argument<FileInfo>(() => null).ExistingOnly()
            });

            Add(new Option(new[] { "-o", "--option" }, "Specify key/value pairs to override project configuration settings.")
            {
                Argument = new Argument<IReadOnlyList<KeyValuePair<string, string>>>(ParseKeyValuePairs)
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
                    
                    if(string.IsNullOrEmpty(key))
                    {
                        // Can't have an empty key.
                        parsed.ErrorMessage = string.Format("Cannot have an empty key value for an option argument: '{0}'", item.Value);
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
                        list.Add(new KeyValuePair<string, string>(key, true.ToString()));
                    }
                }
            }

            return list;
        }
    }
}
