using System;
using Paramore.Brighter;

namespace GreetingsCore.Ports.Commands
{
    public class RegreetEvent : Event
    {
        public string GreetingId { get; set; }

        public RegreetEvent() : base(Guid.NewGuid())
        {
            //required for de-serialization
        }
        
        public RegreetEvent(Guid greetingId) : base(Guid.NewGuid())
        {
            GreetingId = greetingId.ToString();
        }
   }
}