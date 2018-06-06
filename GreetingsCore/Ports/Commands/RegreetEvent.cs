using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GreetingsCore.Adapters.Db;
using GreetingsCore.Model;
using Microsoft.EntityFrameworkCore;

namespace GreetingsCore.Ports.Commands
{
    public class RegreetEvent : IAmAnEvent
    {
       private readonly DbContextOptions<GreetingContext> _options;
       public Guid GreetingId { get; set; }

       public RegreetEvent(Guid greetingId, DbContextOptions<GreetingContext> options)
       {
           _options = options;
           GreetingId = greetingId;
       }

        public async Task ExecuteAsync(CancellationToken ct = new CancellationToken())
        {
                        //Note how we share the Db - same microservice, different process, so 
            // we can just ask for the greeting
            Greeting greeting;
            using (var uow = new GreetingContext(_options))
            {
                greeting = await uow.Greetings.SingleOrDefaultAsync(g => g.Id == GreetingId);
                
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
                Console.WriteLine(GreetingId.ToString());
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
                Console.WriteLine(GreetingId.ToString());
                Console.WriteLine("----------------------------------");
                Console.WriteLine("Message Ends");
            }
 
        }
    }
}