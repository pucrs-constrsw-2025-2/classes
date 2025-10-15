using System.Threading.Tasks;

namespace ClasseMicroservice.API.Queries
{
    public interface IQueryHandler<TQuery, TResult>
    {
        Task<TResult> HandleAsync(TQuery query);
    }
}
