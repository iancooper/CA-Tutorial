using System.Threading;
using System.Threading.Tasks;

namespace GreetingsCore.Ports.Commands
{
    public interface IAmAnEvent
    {
        Task ExecuteAsync(CancellationToken ct);
     }
}