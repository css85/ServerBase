using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Shared.Session.Base;

namespace Shared.ServerApp.Services
{
    public abstract class BaseConsoleCommandService
    {
        private readonly ILogger<BaseConsoleCommandService> _logger;
        private readonly AppContextServiceBase _appContext;
        private Thread _consoleThread;

        private readonly Dictionary<string, (int, Func<string[], Task>)> _commands = new();

        public BaseConsoleCommandService(
            ILogger<BaseConsoleCommandService> logger,
            AppContextServiceBase appContext
        )
        {
            _logger = logger;
            _appContext = appContext;
        }

        public IReadOnlyDictionary<string, (int, Func<string[], Task>)> Commands => _commands;

        public virtual Task StartAsync()
        {
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) &&
               _appContext.IsUnitTest == false)
            {
                _consoleThread = new Thread(StartThread)
                {
                    Name = GetType().Name,
                    IsBackground = true
                };
                _consoleThread.Start();
            }

            return Task.CompletedTask;
        }

        public virtual Task StopAsync()
        {
            if (_consoleThread != null)
                _consoleThread.Interrupt();
            return Task.CompletedTask;
        }

        public void AddCommand(string commandName, int requiredParameterLength, Func<string[], Task> action)
        {
            _commands.Add(commandName, (requiredParameterLength, action));
        }

        [SuppressMessage("ReSharper", "VSTHRD002")]
        private void StartThread()
        {
            while (true)
            {
                try
                {
                    var message = Console.ReadLine();
                    if (message == null)
                        break;

                    if (string.IsNullOrEmpty(message))
                        continue;

                    var commands = ParseMessageToCommands(message);
                    Task.Run(() => ProcessCommandAsync(commands)).Wait();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static string[] ParseMessageToCommands(string message)
        {
            return message.ToLower().Split(' ').Select(p => p.Replace(" ", "")).ToArray();
        }

        private async Task ProcessCommandAsync(string[] commands)
        {
            Console.WriteLine();

            if (_commands.TryGetValue(commands[0], out var command))
            {
                if (CheckParameterLength(commands, command.Item1) == false)
                    return;

                await command.Item2.Invoke(commands);

                return;
            }

            Console.WriteLine();
            Console.WriteLine("Command not found.");
        }

        public static bool CheckParameterLength(string[] commands, int requireLength)
        {
            if (commands == null)
            {
                Console.WriteLine("Commands is null.");
                return false;
            }

            if (commands.Length - 1 < requireLength)
            {
                Console.WriteLine($"Parameter length is short. required: {requireLength}");
                return false;
            }

            return true;
        }
    }
}
