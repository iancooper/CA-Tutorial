using System;
using System.Threading;
using System.Threading.Tasks;
using GreetingsCore.Adapters.Db;
using GreetingsCore.Adapters.Repositories;
using GreetingsCore.Model;
using Microsoft.EntityFrameworkCore;

namespace GreetingsCore.Ports.Facades
{
    public class GreetingFacade
    {
        private readonly DbContextOptions<GreetingContext> _options;

        public GreetingFacade(DbContextOptions<GreetingContext> options)
        {
            _options = options;
        }
        
        public async Task AddAsync(Guid id, string message, CancellationToken cancellationToken = new CancellationToken())
        {
            using (var uow = new GreetingContext(_options))
            {
                var repository = new GreetingRepositoryAsync(uow);
                var savedItem = await repository.AddAsync(new Greeting{Id = id, Message = message},cancellationToken);
            }
        }
        
        public async Task<GreetingsAllResult> AllAsync(CancellationToken cancellationToken= new CancellationToken())
        {
            using (var uow = new GreetingContext(_options))
            {
                var greetings = await uow.Greetings.ToArrayAsync(cancellationToken);
                
                var results = new GreetingsByIdResult[greetings.Length];
                for (var i = 0; i < greetings.Length; i++)
                {
                    results[i] = new GreetingsByIdResult(greetings[i]);
                }
                return new GreetingsAllResult(results);
            }
  
        }
        
        public async Task DeleteAsync(Guid itemToDelete, CancellationToken cancellationToken = new CancellationToken())
        {
            using (var uow = new GreetingContext(_options))
            {
                var repository = new GreetingRepositoryAsync(uow);
                await repository.DeleteAsync(itemToDelete, cancellationToken);
            }
        }
        
        
        public async Task<GreetingsByIdResult> GetAsync(Guid id, CancellationToken cancellationToken = new CancellationToken())
        {  
            using (var uow = new GreetingContext(_options))
            {
                var greeting = await uow.Greetings.SingleAsync(t => t.Id == id, cancellationToken: cancellationToken);
                return new GreetingsByIdResult(greeting);
            }
 
        }
        
        public async Task RegreetAsync(Guid greetingId, CancellationToken ct = new CancellationToken())
        {
            //Note how we share the Db - same microservice, different process, so 
            // we can just ask for the greeting
            Greeting greeting;
            using (var uow = new GreetingContext(_options))
            {
                greeting = await uow.Greetings.SingleOrDefaultAsync(g => g.Id == greetingId);
                
            }

            //we don't want to terminate, so on to the next message
            if (greeting == null)
            {
                Console.WriteLine("Received Greeting. Message Follows");
                Console.WriteLine("----------------------------------");
                Console.WriteLine("Could not read message}");
                Console.WriteLine("----------------------------------");
                Console.WriteLine("Greeting Id from Originator Follows");
                Console.WriteLine("----------------------------------");
                Console.WriteLine(greetingId.ToString());
                Console.WriteLine("----------------------------------");
                Console.WriteLine("Message Ends");
            }
            else
            {

                Console.WriteLine("Received Greeting. Message Follows");
                Console.WriteLine("----------------------------------");
                Console.WriteLine(greeting.Message);
                Console.WriteLine("----------------------------------");
                Console.WriteLine("Greeting Id from Originator Follows");
                Console.WriteLine("----------------------------------");
                Console.WriteLine(greetingId.ToString());
                Console.WriteLine("----------------------------------");
                Console.WriteLine("Message Ends");
            }
 
        }
 
    }
}