using System.Threading;
using System.Threading.Tasks;

namespace GreetingsCore.Ports.Commands
{
    public interface IAmACommand
    {
        Task ExecuteAsync(CancellationToken ct);
    }
}