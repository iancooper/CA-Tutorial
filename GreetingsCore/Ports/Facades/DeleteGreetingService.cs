using System;
using System.Threading;
using System.Threading.Tasks;
using GreetingsCore.Adapters.Db;
using GreetingsCore.Adapters.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GreetingsCore.Ports.Facades
{
    public class DeleteGreetingService 
    {        
        private readonly DbContextOptions<GreetingContext> _options;

        public Guid ItemToDelete { get; }
        
        public DeleteGreetingService(Guid itemToDelete, DbContextOptions<GreetingContext> options)
        {
            _options = options;
            ItemToDelete = itemToDelete;
        }

        public async Task DeleteAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            using (var uow = new GreetingContext(_options))
            {
                var repository = new GreetingRepositoryAsync(uow);
                await repository.DeleteAsync(ItemToDelete, cancellationToken);
            }
        }
    }
}