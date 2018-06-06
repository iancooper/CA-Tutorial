using System;
using Paramore.Brighter;

namespace GreetingsCore.Ports.Commands
{
    public class RegreetCommand : Command
    {
        public string GreetingId { get; set; }

        public RegreetCommand() : base(Guid.NewGuid())
        {
            //required for de-serialization
        }
        
        public RegreetCommand(Guid greetingId) : base(Guid.NewGuid())
        {
            GreetingId = greetingId.ToString();
        }

        public Guid GreetingIdAsGuid()
        {
            //Newtonsoft JSON can't tell it has a GUID, so convert here for it
            return Guid.Parse(GreetingId);
        }
    }
}