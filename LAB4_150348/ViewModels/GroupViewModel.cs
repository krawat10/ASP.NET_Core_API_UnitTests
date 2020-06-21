using System.Collections.Generic;
using DataAccess.Models;

namespace LAB4_150348.ViewModels
{
    public class GroupViewModel
    {
        public int BooksAmount { get; set; }
        public decimal AveragePrice { get; set; }
        public IEnumerable<string> UniqueAuthors { get; set; }
        public IEnumerable<Book> Books { get; set; }
    }
}