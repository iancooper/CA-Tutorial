using System;
using System.Threading.Tasks;
using GreetingsCore.Adapters.Db;
using GreetingsCore.Adapters.ViewModels;
using GreetingsCore.Ports;
using GreetingsCore.Ports.Commands;
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
            var greetings = await new GreetingsAllQuery(_options).ExecuteAsync();
            return Ok(greetings.Greetings);
        }

        [HttpGet("{id}", Name = "GetGreeting")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var greeting = await new GreetingsByIdQuery(id, _options).ExecuteAsync();
            return Ok(greeting);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] AddGreetingRequest request)
        {
            var newGreetingId = Guid.NewGuid();
            var addGreetingCommand = new AddGreetingCommand(newGreetingId, request.Message, _options);

            await addGreetingCommand.ExecuteAsync();
            
            var regreetEvent = new RegreetEvent(addGreetingCommand.Id, _options);
            await regreetEvent.ExecuteAsync();

            var addedGreeting = await new GreetingsByIdQuery(newGreetingId, _options).ExecuteAsync();
            
            return Ok(addedGreeting);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleteGreetingCommand = new DeleteGreetingCommand(id, _options);
            await deleteGreetingCommand.ExecuteAsync();
            return Ok();
        }
    }
}