using System.Threading.Tasks;

namespace ClasseMicroservice.Application.Commands
{
    public interface ICommandHandler<TCommand>
    {
        Task HandleAsync(TCommand command);
    }
}
