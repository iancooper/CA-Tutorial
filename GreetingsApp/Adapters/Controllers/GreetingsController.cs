using System;
using System.Threading.Tasks;
using GreetingsCore.Adapters.ViewModels;
using GreetingsCore.Ports;
using GreetingsCore.Ports.Commands;
using Microsoft.AspNetCore.Mvc;
using Paramore.Brighter;
using Paramore.Darker;

namespace GreetingsApp.Adapters.Controllers
{
    [Route("api/[controller]")]
    public class GreetingsController : Controller
    {
        private readonly IAmACommandProcessor _commandProcessor;
        private readonly IQueryProcessor _queryProcessor;
        
        
        // GET
        public GreetingsController(IAmACommandProcessor commandProcessor, IQueryProcessor queryProcessor)
        {
            _commandProcessor = commandProcessor;
            _queryProcessor = queryProcessor;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var greetings = await _queryProcessor.ExecuteAsync(new GreetingsAllQuery());
            return Ok(greetings.Greetings);
        }

        [HttpGet("{id}", Name = "GetGreeting")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var greeting = await _queryProcessor.ExecuteAsync(new GreetingsByIdQuery(id));
            return Ok(greeting);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] AddGreetingRequest request)
        {
            var newGreetingId = Guid.NewGuid();
            var addGreetingCommand = new AddGreetingCommand(newGreetingId, request.Message);

            await _commandProcessor.SendAsync(addGreetingCommand);

            var addedGreeting = await _queryProcessor.ExecuteAsync(new GreetingsByIdQuery(newGreetingId));
            
            return Ok(addedGreeting);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleteGreetingCommand = new DeleteGreetingCommand(id);
            await _commandProcessor.SendAsync(deleteGreetingCommand);
            return Ok();
        }
    }
}