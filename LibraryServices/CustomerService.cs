using GreaterSydneyLibraries.Models;
using LibraryData;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibraryServices
{
    public class CustomerService : ICustomer
    {
        private readonly LibraryContext _context;

        public CustomerService( LibraryContext context)
        {
            _context = context;

        }
        public void Add(Customers newCustomer)
        {
            _context.Add(newCustomer);
            _context.SaveChanges();
        }

        public Customers Get(int id)
        {
            return GetAll()
                .FirstOrDefault(Customers => Customers.Id == id);
        }

        public IEnumerable<Customers> GetAll()
        {
            return _context.Customers
                   .Include(Customers => Customers.LibraryCard)
                   .Include(Customers => Customers.LocalBranch);
        }

        public IEnumerable<CheckoutHistory> GetCheckoutHistory(int id)
        {
            var cardId = Get(id).LibraryCard.Id;
            return _context.CheckoutHistories
                    .Include(c => c.LibraryCard)
                    .Include(c => c.LibraryAsset)
                    .Where(c => c.LibraryCard.Id == cardId)
                    .OrderByDescending(c => c.CheckOut);
        }

        public IEnumerable<Checkouts> GetCheckouts(int id)
        {
            var cardId = Get(id).LibraryCard.Id;
            return _context.Checkouts
                    .Include(c => c.LibraryCard)
                    .Include(c => c.LibraryAsset)
                    .Where(c => c.LibraryCard.Id == cardId);   // LibraryCard is navigatoin prop
        }

        public IEnumerable<Holds> GetHolds(int id)
        {
            var cardId = Get(id).LibraryCard.Id;
            return _context.Holds
                    .Include(h => h.LibraryCard)
                    .Include(h => h.LibraryAsset)
                    .Where(h => h.LibraryCard.Id == cardId)
                    .OrderByDescending(h => h.HoldPlaced);
        }
    }
}

