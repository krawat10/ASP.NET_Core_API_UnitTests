using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ExcelMapper;

namespace DataAccess.Models
{
    public class Book
    {
        [Key]
        [ExcelIgnore]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public int AvailableAmount { get; set; }
        public decimal Price { get; set; }
        [ExcelIgnore]
        public IEnumerable<BookOrder> BookOrders { get; set; }

        public Book()
        {
            BookOrders = new List<BookOrder>();
        }
    }
}