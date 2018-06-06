using System;
using System.Threading;
using System.Threading.Tasks;
using GreetingsCore.Adapters.Db;
using GreetingsCore.Adapters.Repositories;
using GreetingsCore.Model;
using Microsoft.EntityFrameworkCore;

namespace GreetingsCore.Ports.Commands
{
    public class AddGreetingCommand : IAmACommand
    {
        private readonly DbContextOptions<GreetingContext> _options;
        
        public Guid Id { get; }
        public string Message { get; }

        public AddGreetingCommand(Guid id, string message, DbContextOptions<GreetingContext> options) 
        {
            _options = options;
            Id = id;
            Message = message;
        }


        public async Task ExecuteAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            using (var uow = new GreetingContext(_options))
            {
                var repository = new GreetingRepositoryAsync(uow);
                var savedItem = await repository.AddAsync(new Greeting{Id = Id, Message = Message},cancellationToken);
            }
        }
    }
}