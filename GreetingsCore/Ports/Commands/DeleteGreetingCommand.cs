using System;
using System.Threading;
using System.Threading.Tasks;
using GreetingsCore.Adapters.Db;
using GreetingsCore.Adapters.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GreetingsCore.Ports.Commands
{
    public class DeleteGreetingCommand : IAmACommand
    {        
        private readonly DbContextOptions<GreetingContext> _options;

        public Guid ItemToDelete { get; }
        
        public DeleteGreetingCommand(Guid itemToDelete, DbContextOptions<GreetingContext> options)
        {
            _options = options;
            ItemToDelete = itemToDelete;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            using (var uow = new GreetingContext(_options))
            {
                var repository = new GreetingRepositoryAsync(uow);
                await repository.DeleteAsync(ItemToDelete, cancellationToken);
            }
        }
    }
}