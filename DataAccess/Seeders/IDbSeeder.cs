using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Seeders
{
    public interface IDbSeeder<in T> where T : DbContext
    {
        public Task<bool> Seed(T context);
    }
}