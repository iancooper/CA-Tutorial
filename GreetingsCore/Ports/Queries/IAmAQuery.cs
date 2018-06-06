using System.Threading;
using System.Threading.Tasks;

namespace GreetingsCore.Ports
{
    public interface IAmAQuery<TResult>
    {
        Task<TResult> ExecuteAsync(CancellationToken ct);
     }
}