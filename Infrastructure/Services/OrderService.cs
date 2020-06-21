using System.Threading.Tasks;
using DataAccess;
using DataAccess.Models;

namespace Infrastructure.Services
{
    public class OrderService: IOrderService
    {
        private readonly ApplicationDbContext _context;

        public OrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddBookToOrder(Order order, Book book, int amount)
        {
            order.BookOrders.Add(new BookOrder
            {
                Book = book,
                Order = order,
                BookAmount = amount
            });

            order.TotalPrice += book.Price * amount;

            await _context.SaveChangesAsync();
        }
    }
}