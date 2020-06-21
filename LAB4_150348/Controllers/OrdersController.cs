using System.Threading.Tasks;
using DataAccess.Models;
using DataAccess.Repositories;
using Infrastructure.Services;
using LAB4_150348.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LAB4_150348.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<Book> _bookRepository;

        public OrdersController(IOrderService orderService, IRepository<Order> orderRepository, IRepository<Book> bookRepository)
        {
            _orderService = orderService;
            _orderRepository = orderRepository;
            _bookRepository = bookRepository;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            return View(await _orderRepository.GetAll());
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _orderRepository.Get(id.Value);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Orders/Create
        public async Task<IActionResult> Create(int? id)
        {
            ViewBag.Books = await _bookRepository.GetAll();

            if (id.HasValue)
            {
                var order = await _orderRepository.Get(id.Value);
                return View(order);
            }

            var entity = new Order();
            await _orderRepository.Create(entity);

            return View(entity);
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit([Bind("Id")] int id)
        {
            if (ModelState.IsValid)
            {
                var order = await _orderRepository.Get(id);

                if (order == null)
                {
                    return NotFound();
                }

                if (order.TotalPrice <= 40)
                {
                    return BadRequest("Order price must be greater than 40.");
                }

                order.FinishOrder();

                await _orderRepository.Update(order);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _orderRepository.Get(id.Value);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Created,TotalPrice")] Order order)
        {
            if (id != order.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _orderRepository.Update(order);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await OrderExists(order.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(order);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _orderRepository.Get(id.Value);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _orderRepository.Remove(id);

            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> OrderExists(int id)
        {
            return await _orderRepository.Exists(id);
        }

        public async Task<IActionResult> AddBookToOrder([Bind("BookId,BookAmount,OrderId")] BookOrderViewModel model)
        {
            if (ModelState.IsValid)
            {
                var order = await _orderRepository.Get(model.OrderId);
                var book = await _bookRepository.Get(model.BookId);

                if (book == null) return BadRequest($"Book with id {model.BookId} does not exists.");
                if (order == null) return BadRequest($"Order with id {model.OrderId} does not exists.");

                if (model.BookAmount > book.AvailableAmount)
                {
                    return BadRequest("Cannot order more books than are available.");
                }

                await _orderService.AddBookToOrder(order, book, model.BookAmount);
            }

            return RedirectToAction("Create", new { id = model.OrderId });
        }
    }
}
