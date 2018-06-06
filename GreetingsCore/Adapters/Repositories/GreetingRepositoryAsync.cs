using System;
using System.Threading;
using System.Threading.Tasks;
using GreetingsCore.Adapters.Db;
using GreetingsCore.Model;
using GreetingsCore.Ports.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GreetingsCore.Adapters.Repositories
{
    public class GreetingRepositoryAsync : IRepositoryAsync<Greeting>
    {
        private readonly GreetingContext _uow;

        public GreetingRepositoryAsync(GreetingContext uow)
        {
            _uow = uow;
        }

        public async Task<Greeting> AddAsync(Greeting newEntity, CancellationToken ct = default(CancellationToken) )
        {
            var savedItem = _uow.Greetings.Add(newEntity);
            await _uow.SaveChangesAsync(ct);
            return savedItem.Entity;
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct= default(CancellationToken))
        {
            var toDoItem = await _uow.Greetings.SingleAsync(t => t.Id == id, ct);
            _uow.Remove(toDoItem);
            await _uow.SaveChangesAsync(ct);
    }

        public async Task DeleteAllAsync(CancellationToken ct = default(CancellationToken))
        {
            _uow.Greetings.RemoveRange(await _uow.Greetings.ToListAsync(ct));
            await _uow.SaveChangesAsync(ct);
         }

        public async Task<Greeting> GetAsync(Guid entityId, CancellationToken ct = new CancellationToken())
        {
            return await _uow.Greetings.SingleAsync(t => t.Id == entityId, ct);
         }

        public async Task UpdateAsync(Greeting updatedEntity, CancellationToken ct = new CancellationToken())
        {
             await _uow.SaveChangesAsync(ct);
        }
    }
}