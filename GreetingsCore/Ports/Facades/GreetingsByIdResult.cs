using System;
using GreetingsCore.Model;

namespace GreetingsCore.Ports.Facades
{
    public class GreetingsByIdResult
    {
        public Guid Id { get; }

        public string Message { get; }
        
        public GreetingsByIdResult(Greeting greeting)
        {
            Id = greeting.Id;
            Message = greeting.Message;
        }

    }
}