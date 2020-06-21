using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories
{
    public class BookRepository : IRepository<Book>
    {
        private readonly ApplicationDbContext _context;

        public BookRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<Book> Get(int id)
        {
            throw new System.NotImplementedException();
        }

        public async Task<bool> Exists(int id)
        {
            return await _context.Orders.AnyAsync(e => e.Id == id);
        }

        public async Task<ICollection<Book>> GetAll()
        {
            return await _context.Books.ToListAsync();
        }


        public async Task Create(Book entry)
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

        public async Task Update(Book entry)
        {
            _context.Update(entry);
            await _context.SaveChangesAsync();
        }
    }
}