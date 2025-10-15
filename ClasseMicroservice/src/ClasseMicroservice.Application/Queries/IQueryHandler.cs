using System.Threading.Tasks;

namespace ClasseMicroservice.Application.Queries
{
    public interface IQueryHandler<TQuery, TResult>
    {
        Task<TResult> HandleAsync(TQuery query);
    }
}
