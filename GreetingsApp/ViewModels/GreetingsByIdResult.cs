using System;
using GreetingsApp.Model;

namespace GreetingsApp.ViewModels
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