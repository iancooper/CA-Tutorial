using System;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using GreetingsCore.Adapters.Db;
using GreetingsCore.Model;
using Microsoft.EntityFrameworkCore;

namespace GreetingsCore.Ports
{
    public class GreetingsByIdQuery: IAmAQuery<GreetingsByIdResult>
    {
        private readonly DbContextOptions<GreetingContext> _options;
        public Guid Id { get; }
        
        public GreetingsByIdQuery(Guid id, DbContextOptions<GreetingContext> options)
        {
            _options = options;
            Id = id;
        }

        public async Task<GreetingsByIdResult> ExecuteAsync(CancellationToken cancellationToken = new CancellationToken())
        {  
            using (var uow = new GreetingContext(_options))
            {
                var greeting = await uow.Greetings.SingleAsync(t => t.Id == Id, cancellationToken: cancellationToken);
                return new GreetingsByIdResult(greeting);
            }
 
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