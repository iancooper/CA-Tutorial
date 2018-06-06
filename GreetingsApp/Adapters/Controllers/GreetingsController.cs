using System;
using System.Threading;
using System.Threading.Tasks;
using GreetingsCore.Db;
using GreetingsCore.Model;
using GreetingsCore.Repositories;
using GreetingsCore.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GreetingsApp.Adapters.Controllers
{
    [Route("api/[controller]")]
    public class GreetingsController : Controller
    {
        private readonly DbContextOptions<GreetingContext> _options;
        
        public GreetingsController(DbContextOptions<GreetingContext> options)
        {
            _options = options;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            GreetingsAllResult greetings;
            using (var uow = new GreetingContext(_options))
            {
                var greetings1 = await uow.Greetings.ToArrayAsync();
                
                var results = new GreetingsByIdResult[greetings1.Length];
                for (var i = 0; i < greetings1.Length; i++)
                {
                    results[i] = new GreetingsByIdResult(greetings1[i]);
                }
                
                greetings = new GreetingsAllResult(results); 
            }

            return Ok(greetings.Greetings);
        }

        [HttpGet("{id}", Name = "GetGreeting")]
        public async Task<IActionResult> GetById(Guid id)
        {
            GreetingsByIdResult greeting;
            using (var uow = new GreetingContext(_options))
            {
                var greeting1 = await uow.Greetings.SingleAsync(t => t.Id == id);
                greeting = new GreetingsByIdResult(greeting1); 
            }

            return Ok(greeting);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] AddGreetingRequest request)
        {
            var newGreetingId = Guid.NewGuid();
            
            Console.WriteLine("Received Greeting. Message Follows");
            Console.WriteLine("----------------------------------");
            Console.WriteLine(request.Message);
            Console.WriteLine("----------------------------------");
            Console.WriteLine("Greeting Id Follows");
            Console.WriteLine("----------------------------------");
            Console.WriteLine(newGreetingId.ToString());
            Console.WriteLine("----------------------------------");
            Console.WriteLine("Greeting Ends");
            
            using (var uow1 = new GreetingContext(_options))
            {
                var repository = new GreetingRepositoryAsync(uow1);
                await repository.AddAsync(new Greeting{Id = newGreetingId, Message = request.Message},new CancellationToken());
            }

            GreetingsByIdResult greeting;
            using (var uow = new GreetingContext(_options))
            {
                var greeting1 = await uow.Greetings.SingleAsync(t => t.Id == newGreetingId);
                greeting = new GreetingsByIdResult(greeting1); 
            }

            return Ok(greeting);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            using (var uow = new GreetingContext(_options))
            {
                var repository = new GreetingRepositoryAsync(uow);
                await repository.DeleteAsync(id, new CancellationToken());
            }

            return Ok();
        }
    }
}