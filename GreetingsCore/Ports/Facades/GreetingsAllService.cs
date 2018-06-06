using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GreetingsCore.Adapters.Db;
using Microsoft.EntityFrameworkCore;

namespace GreetingsCore.Ports.Facades
{
    public class GreetingsAllService
    {        
        private readonly DbContextOptions<GreetingContext> _options;
 
        public GreetingsAllService(DbContextOptions<GreetingContext> options)
        {
            _options = options;
        }

        public async Task<GreetingsAllResult> QueryAsync(CancellationToken cancellationToken= new CancellationToken())
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

    public class GreetingsAllResult
    {
        public IEnumerable<GreetingsByIdResult> Greetings { get; }

        public GreetingsAllResult(IEnumerable<GreetingsByIdResult> greetings )
        {
            Greetings = greetings;
        }
    }
}