using System;
using Paramore.Brighter;

namespace GreetingsCore.Ports.Commands
{
    public class DeleteGreetingCommand : Command
    {
        public Guid ItemToDelete { get; }
        
        public DeleteGreetingCommand(Guid itemToDelete) : this(Guid.NewGuid(), itemToDelete){}

        public DeleteGreetingCommand(Guid id, Guid itemToDelete) : base(id)
        {
            ItemToDelete = itemToDelete;
        }
   }
}