using System.Threading;
using System.Threading.Tasks;
using GreetingsCore.Adapters.Db;
using Microsoft.EntityFrameworkCore;
using Paramore.Darker;

namespace GreetingsCore.Ports.Handlers
{
    public class GreetingsAllHandlerAsync : QueryHandlerAsync<GreetingsAllQuery, GreetingsAllResult>
    {
        private readonly DbContextOptions<GreetingContext> _options;

        public GreetingsAllHandlerAsync(DbContextOptions<GreetingContext> options)
        {
            _options = options;
        }

        public override async Task<GreetingsAllResult> ExecuteAsync(GreetingsAllQuery query, CancellationToken cancellationToken = new CancellationToken())
        {
            using (var uow = new GreetingContext(_options))
            {
                var greetings = await uow.Greetings.ToArrayAsync(cancellationToken);
                
                var results = new GreetingsByIdResult[greetings.Length];
                for (var i = 0; i < greetings.Length; i++)
                {
                    results[i] = new GreetingsByIdResult(greetings[i]);
                }
                return new GreetingsAllResult(results);
            }
        }
    }
}