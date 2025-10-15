using System.Threading.Tasks;

namespace ClasseMicroservice.API.Commands
{
    public interface ICommandHandler<TCommand>
    {
        Task HandleAsync(TCommand command);
    }
}
