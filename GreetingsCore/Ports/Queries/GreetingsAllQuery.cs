using System.Collections.Generic;
using Paramore.Darker;

namespace GreetingsCore.Ports
{
    public class GreetingsAllQuery: IQuery<GreetingsAllResult>
    {
        
    }

    public class GreetingsAllResult
    {
        public IEnumerable<GreetingsByIdResult> Greetings { get; }

        public GreetingsAllResult(IEnumerable<GreetingsByIdResult> greetings )
        {
            Greetings = greetings;
        }
    }
}