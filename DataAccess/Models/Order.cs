using System;
using System.Collections.Generic;

namespace DataAccess.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime Created { get; set; }
        public decimal TotalPrice { get; set; }
        public ICollection<BookOrder> BookOrders { get; set; }

        public Order()
        {
            BookOrders = new List<BookOrder>();
            Created = DateTime.Now;
            IsFinished = false;
        }

        public bool IsFinished { get; set; }

        public void FinishOrder()
        {
            foreach (var bookOrder in BookOrders)
            {
                bookOrder.Book.AvailableAmount -= bookOrder.BookAmount;
            }

            IsFinished = true;
        }

    }
}