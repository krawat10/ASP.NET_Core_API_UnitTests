using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public interface IRepository<T> where  T: class
    {
        Task<T> Get(int id);
        Task<bool> Exists(int id);
        Task<ICollection<T>> GetAll();

        Task Create(T entry);
        Task Remove(int id);
        Task Update(T entry);

    }
}