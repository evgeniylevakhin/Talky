﻿using System.Collections.Generic;
using System.Linq;

namespace Server.Command
{
    class CommandManager
    {
        public static CommandManager Instance { get; private set; } = new CommandManager();
        private readonly Dictionary<string, TalkyCommand> _commands = new Dictionary<string, TalkyCommand>();

        private readonly object _lock = new object();

        private CommandManager() { }

        public void RegisterCommand(TalkyCommand command)
        {
            lock (_lock)
            {
                if (!_commands.ContainsValue(command) && !_commands.ContainsKey(command.CommandName.ToLower()))
                {
                    _commands.Add(command.CommandName.ToLower(), command);
                }
            }
        }

        public void UnregisterCommand(TalkyCommand command)
        {
            UnregisterCommand(command.CommandName);
        }

        public void UnregisterCommand(string name)
        {
            lock (_lock)
            {
                _commands.Remove(name);
            }
        }

        public TalkyCommand Get(string command)
        {
            lock (_lock)
            {
                _commands.TryGetValue(command.ToLower(), out var theCommand);
                return theCommand;
            }
        }

        public IReadOnlyList<TalkyCommand> All()
        {
            List<TalkyCommand> commands = new List<TalkyCommand>();
            lock (_lock)
            {
                foreach (string key in _commands.Keys)
                {
                    commands.Add(Get(key));
                }
            }
            return commands.AsReadOnly();
        }

        public int Count()
        {
            lock (_commands)
            {
                return _commands.Count();
            }
        }
    }
}
