using System;
using System.Threading;
using System.Threading.Tasks;

namespace GreetingsCore.Ports.Repositories
{
    public interface IRepositoryAsync<T> where T : IEntity
    {
        Task<T> AddAsync(T newEntity, CancellationToken ct = default(CancellationToken));
        Task DeleteAsync(Guid id, CancellationToken ct = default(CancellationToken));
        Task DeleteAllAsync(CancellationToken ct = default(CancellationToken));
        Task<T> GetAsync(Guid id, CancellationToken ct = default(CancellationToken));
        Task UpdateAsync(T updatedEntity, CancellationToken ct = default(CancellationToken));
    }
}