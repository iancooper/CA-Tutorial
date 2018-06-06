using System.Threading;
using System.Threading.Tasks;
using GreetingsCore.Adapters.Db;
using Microsoft.EntityFrameworkCore;
using Paramore.Darker;
using Paramore.Darker.Policies;
using Paramore.Darker.QueryLogging;

namespace GreetingsCore.Ports.Handlers
{
    public class GreetingByIdHandlerAsync : QueryHandlerAsync<GreetingsByIdQuery, GreetingsByIdResult>
    {
        private readonly DbContextOptions<GreetingContext> _options;

        public GreetingByIdHandlerAsync(DbContextOptions<GreetingContext> options)
        {
            _options = options;
        }

        [QueryLogging(1)]
        [RetryableQuery(2)]
        public override async Task<GreetingsByIdResult> ExecuteAsync(GreetingsByIdQuery query, CancellationToken cancellationToken = new CancellationToken())
        {
            using (var uow = new GreetingContext(_options))
            {
                var greeting = await uow.Greetings.SingleAsync(t => t.Id == query.Id, cancellationToken: cancellationToken);
                return new GreetingsByIdResult(greeting);
            }
  
        }
    }
}