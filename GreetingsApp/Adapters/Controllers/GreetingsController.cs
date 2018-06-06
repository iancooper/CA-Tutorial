using System;
using System.Threading.Tasks;
using GreetingsCore.Adapters.Db;
using GreetingsCore.Adapters.ViewModels;
using GreetingsCore.Ports.Facades;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GreetingsApp.Adapters.Controllers
{
    [Route("api/[controller]")]
    public class GreetingsController : Controller
    {
        private readonly GreetingFacade _facade;
        
        public GreetingsController(DbContextOptions<GreetingContext> options)
        {
            _facade = new GreetingFacade(options);
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var greetings = await _facade.AllAsync(); 
            return Ok(greetings.Greetings);
        }

        [HttpGet("{id}", Name = "GetGreeting")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var greeting = await _facade.GetAsync(id); 
            return Ok(greeting);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] AddGreetingRequest request)
        {
            var newGreetingId = Guid.NewGuid();
            await _facade.AddAsync(newGreetingId, request.Message);

            await _facade.RegreetAsync(newGreetingId);

            var addedGreeting = await _facade.GetAsync(newGreetingId); 
            
            return Ok(addedGreeting);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _facade.DeleteAsync(id);
            return Ok();
        }
    }
}