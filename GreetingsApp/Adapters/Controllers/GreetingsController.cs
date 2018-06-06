using System;
using System.Threading.Tasks;
using GreetingsCore.Adapters.Db;
using GreetingsCore.Adapters.ViewModels;
using GreetingsCore.Ports;
using GreetingsCore.Ports.Commands;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Paramore.Darker;

namespace GreetingsApp.Adapters.Controllers
{
    [Route("api/[controller]")]
    public class GreetingsController : Controller
    {
        private readonly IQueryProcessor _queryProcessor;
        private readonly DbContextOptions<GreetingContext> _options;
        
        public GreetingsController(IQueryProcessor queryProcessor, DbContextOptions<GreetingContext> options)
        {
            _queryProcessor = queryProcessor;
            _options = options;
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
            var addGreetingCommand = new AddGreetingCommand(newGreetingId, request.Message, _options);

            await addGreetingCommand.ExecuteAsync();
            
            var regreetEvent = new RegreetEvent(addGreetingCommand.Id, _options);
            await regreetEvent.ExecuteAsync();

            var addedGreeting = await _queryProcessor.ExecuteAsync(new GreetingsByIdQuery(newGreetingId));
            
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