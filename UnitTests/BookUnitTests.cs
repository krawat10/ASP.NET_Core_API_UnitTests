using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using DataAccess;
using DataAccess.Models;
using DataAccess.Seeders;
using LAB4_150348.Controllers;
using LAB4_150348.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace UnitTests
{
    public class BookUnitTests
    {
        private ApplicationDbContext _context;
        private BooksController _booksController;

        [SetUp]
        public void Setup()
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseInMemoryDatabase(nameof(ApplicationDbContext));
            _context = new ApplicationDbContext(builder.Options);
            _booksController = new BooksController(_context, new ApplicationDbContextSeeder());
        }

        [TestCase("The")]
        public async Task GroupShouldReturnOnlyBooksWithGivenKeyword(string keyword)
        {
            // Arrange
            var books = new List<Book>
            {
                new Book {Author = "JK Rowling", Title = "The Harry Potter", AvailableAmount = 10, Price = 100},
                new Book {Author = "Tokien", Title = "The Lord of ring", AvailableAmount = 15, Price = 90},
                new Book {Author = "Frank D. Luna", Title = "3D Game Programing with DirectX 12", AvailableAmount = 5, Price = 110},
            };

            var booksWithKeyword = books.Where(book => book.Title.ToLower().Contains(keyword.ToLower()));

            await _context.AddRangeAsync(books);
            await _context.SaveChangesAsync();

            // Act
            var result = _booksController.Group(keyword) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsInstanceOf<GroupViewModel>(result.Model);
            Assert.AreEqual((result.Model as GroupViewModel).BooksAmount, booksWithKeyword.Count());
            Assert.AreEqual((result.Model as GroupViewModel).UniqueAuthors, booksWithKeyword.Select(book => book.Author).Distinct());
            Assert.AreEqual((result.Model as GroupViewModel).AveragePrice, booksWithKeyword.Select(book => book.Price).Average());
            CollectionAssert.AreEquivalent((result.Model as GroupViewModel).Books, booksWithKeyword);
        }


        [TearDown]
        public async Task Teardown()
        {
            await _context.Database.EnsureDeletedAsync();
        }
    }
}