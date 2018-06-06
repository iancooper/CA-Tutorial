using System.Threading;
using System.Threading.Tasks;
using GreetingsCore.Adapters.Db;
using GreetingsCore.Adapters.Repositories;
using GreetingsCore.Ports.Commands;
using Microsoft.EntityFrameworkCore;
using Paramore.Brighter;

namespace GreetingsCore.Ports.Handlers
{
    public class DeleteGreetingCommandHandlerAsync : RequestHandlerAsync<DeleteGreetingCommand>
    {
        private readonly DbContextOptions<GreetingContext> _options;

        public DeleteGreetingCommandHandlerAsync(DbContextOptions<GreetingContext> options)
        {
            _options = options;
        }

        public override async Task<DeleteGreetingCommand> HandleAsync(DeleteGreetingCommand command, CancellationToken cancellationToken = new CancellationToken())
        {
            using (var uow = new GreetingContext(_options))
            {
                var repository = new GreetingRepositoryAsync(uow);
                await repository.DeleteAsync(command.Id, cancellationToken);
            }
                
            
            return await base.HandleAsync(command, cancellationToken);
        }
    }
}