using System;
using Paramore.Brighter;

namespace GreetingsCore.Ports.Commands
{
    public class AddGreetingCommand : Command
    {
        public string Message { get; }

        public AddGreetingCommand(Guid id, string message) : base(id)
        {
            Message = message;
        }

        public AddGreetingCommand(string message) : this(Guid.NewGuid(), message) {}
    }
}