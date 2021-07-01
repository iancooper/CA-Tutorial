using System.Collections.Generic;

namespace GreetingsApp.ViewModels
{
    public class GreetingsAllResult
    {
        public IEnumerable<GreetingsByIdResult> Greetings { get; }

        public GreetingsAllResult(IEnumerable<GreetingsByIdResult> greetings )
        {
            Greetings = greetings;
        }
    }
}