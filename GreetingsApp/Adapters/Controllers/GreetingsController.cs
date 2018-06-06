using System;
using System.Threading.Tasks;
using GreetingsCore.Adapters.Db;
using GreetingsCore.Adapters.ViewModels;
using GreetingsCore.Ports;
using GreetingsCore.Ports.Facades;
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
            var greetings = await new GreetingsAllService(_options).QueryAsync();
            return Ok(greetings.Greetings);
        }

        [HttpGet("{id}", Name = "GetGreeting")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var greeting = await new GreetingsByIdService(id, _options).QueryAsync();
            return Ok(greeting);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] AddGreetingRequest request)
        {
            var newGreetingId = Guid.NewGuid();
            var addGreetingCommand = new AddGreetingService(newGreetingId, request.Message, _options);

            await addGreetingCommand.AddAsync();
            
            var regreetEvent = new RegreetService(addGreetingCommand.Id, _options);
            await regreetEvent.RegreetAsync();

            var addedGreeting = await new GreetingsByIdService(newGreetingId, _options).QueryAsync();
            
            return Ok(addedGreeting);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleteGreetingCommand = new DeleteGreetingService(id, _options);
            await deleteGreetingCommand.DeleteAsync();
            return Ok();
        }
    }
}