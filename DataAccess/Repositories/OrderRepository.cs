using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories
{
    public class OrderRepository : IRepository<Order>
    {
        private readonly ApplicationDbContext _context;

        public OrderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Order> Get(int id)
        {
            return await _context.Orders
                .Include(o => o.BookOrders)
                .ThenInclude(bookOrder => bookOrder.Book)
                .FirstOrDefaultAsync(order => order.Id == id);
        }

        public async Task<bool> Exists(int id)
        {
            return await _context.Orders.AnyAsync(e => e.Id == id);
        }

        public async Task<ICollection<Order>> GetAll()
        {
            return await _context.Orders
                .Include(o => o.BookOrders)
                .ThenInclude(bookOrder => bookOrder.Book)
                .ToListAsync();
        }

        public async Task Create(Order entry)
        {
            _context.Add(entry);
            await _context.SaveChangesAsync();
        }

        public async Task Remove(int id)
        {
            var entry = await _context.Orders.FindAsync(id);
            _context.Orders.Remove(entry);
            await _context.SaveChangesAsync();
        }

        public async Task Update(Order entry)
        {
            _context.Update(entry);
            await _context.SaveChangesAsync();
        }
    }
}