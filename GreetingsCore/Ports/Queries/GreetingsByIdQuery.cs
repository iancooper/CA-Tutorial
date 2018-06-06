using System;
using GreetingsCore.Model;
using Paramore.Darker;

namespace GreetingsCore.Ports
{
    public class GreetingsByIdQuery: IQuery<GreetingsByIdResult>
    {
        public Guid Id { get; }
        
        public GreetingsByIdQuery(Guid id)
        {
            Id = id;
        }

    }

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