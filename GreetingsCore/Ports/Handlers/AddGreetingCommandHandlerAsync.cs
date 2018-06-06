using System.Threading;
using System.Threading.Tasks;
using GreetingsCore.Adapters.Db;
using GreetingsCore.Adapters.Repositories;
using GreetingsCore.Model;
using GreetingsCore.Ports.Commands;
using Microsoft.EntityFrameworkCore;
using Paramore.Brighter;
using Paramore.Brighter.Logging.Attributes;
using Paramore.Brighter.Policies.Attributes;

namespace GreetingsCore.Ports.Handlers
{
    public class AddGreetingCommandHandlerAsync : RequestHandlerAsync<AddGreetingCommand>
    {
        private readonly DbContextOptions<GreetingContext> _options;

        public AddGreetingCommandHandlerAsync(DbContextOptions<GreetingContext> options)
        {
            _options = options;
        }

        [RequestLoggingAsync(step: 1, timing: HandlerTiming.Before)]
        [UsePolicyAsync(policy: CommandProcessor.CIRCUITBREAKERASYNC, step:2)]
        [UsePolicyAsync(policy: CommandProcessor.RETRYPOLICYASYNC, step: 3)]
         public override async Task<AddGreetingCommand> HandleAsync(AddGreetingCommand command, CancellationToken cancellationToken = new CancellationToken())
        {
            using (var uow = new GreetingContext(_options))
            {
                var repository = new GreetingRepositoryAsync(uow);
                var savedItem = await repository.AddAsync(
                    new Greeting{Id = command.Id, Message = command.Message},
                    cancellationToken
                );
            }

            return await base.HandleAsync(command, cancellationToken);
        }
    }
}