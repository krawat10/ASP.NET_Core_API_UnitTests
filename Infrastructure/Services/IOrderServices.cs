using System.Threading.Tasks;
using DataAccess.Models;

namespace Infrastructure.Services
{
    public interface IOrderService
    {
        Task AddBookToOrder(Order order, Book book, int amount);
    }
}