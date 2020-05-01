using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using GreaterSydneyLibraries.Models;
using LibraryData;
using Microsoft.EntityFrameworkCore;

namespace LibraryServices
{
    public class CheckoutService : ICheckout
    {
        private readonly LibraryContext _context;

        public CheckoutService(LibraryContext context)
        {
            _context = context;
        }

        public void Add(Checkouts newCheckout)
        {
            _context.Add(newCheckout);
            _context.SaveChanges();
        }

        public Checkouts GetById(int id)
        {
            return _context.Checkouts.FirstOrDefault(p => p.Id == id);
        }

        public IEnumerable<Checkouts> GetAll()
        {
            return _context.Checkouts;
        }

        public void CheckoutItem(int id, int libraryCardId)
        {
            if (IsCheckedOut(id)) return;

            var item = _context.LibraryAssets
                .Include(a => a.Status)
                .First(a => a.Id == id);

            _context.Update(item);

            UpdateAssetStatus(id, "Checked Out");
             

            var now = DateTime.Now;

            var libraryCard = _context.LibraryCards
                .Include(c => c.Checkouts)
                .FirstOrDefault(a => a.Id == libraryCardId);

            var checkout = new Checkouts
            {
                LibraryAsset = item,
                LibraryCard = libraryCard,
                BorrowDate = now,
                ReturnDate = GetDefaultCheckoutTime(now)
            };

            _context.Add(checkout);

            var checkoutHistory = new CheckoutHistory
            {
                CheckOut = now,
                LibraryAsset = item,
                LibraryCard = libraryCard
            };

            _context.Add(checkoutHistory);
            _context.SaveChanges();
        }

        public void MarkLost(int id)
        {
            UpdateAssetStatus(id, "Lost");

            _context.SaveChanges();
        }

        public void MarkFound(int id)
        {
            


            var now = DateTime.Now;

            //update asset status
            UpdateAssetStatus(id, "Available");
            // remove any existing checkouts on the item
             RemoveExistingCheckouts(id);

            // close any existing checkout history
            CloseExistingCheckoutHistory(id, now);
            

            _context.SaveChanges();
        }

        private void UpdateAssetStatus(int id, string v)
        {
            var item = _context.LibraryAssets
                .First(a => a.Id == id);

            _context.Update(item);
            item.Status = _context.Statuses.FirstOrDefault(a => a.Name == "Available");
        }

        private void CloseExistingCheckoutHistory(int id , DateTime now)
        {
            var history = _context.CheckoutHistories
                .FirstOrDefault(h =>
                    h.LibraryAsset.Id == id
                    && h.CheckIn == null);
            if (history != null)
            {
                _context.Update(history);
                history.CheckIn = now;
            }
        }

        private void RemoveExistingCheckouts(int id)
        {
            var checkout = _context.Checkouts
                .FirstOrDefault(a => a.LibraryAsset.Id == id);
            if (checkout != null)
            {
                _context.Remove(checkout);
            }
        }

       

        public void PlaceHold(int assetId, int libraryCardId)
        {
            var now = DateTime.Now;

            var asset = _context.LibraryAssets
                .Include(a => a.Status)
                .First(a => a.Id == assetId);

            var card = _context.LibraryCards
                .First(a => a.Id == libraryCardId);

            _context.Update(asset);

            if (asset.Status.Name == "Available")
                asset.Status = _context.Statuses.FirstOrDefault(a => a.Name == "On Hold");

            var hold = new Holds
            {
                HoldPlaced = now,
                LibraryAsset = asset,
                LibraryCard = card
            };

            _context.Add(hold);
            _context.SaveChanges();
        }

        public void CheckInItem(int id , int libraryCardId)
        {
            var now = DateTime.Now;

            var item = _context.LibraryAssets
                .First(a => a.Id == id);

            _context.Update(item);

            // remove any existing checkouts on the item
            var checkout = _context.Checkouts
                .Include(c => c.LibraryAsset)
                .Include(c => c.LibraryCard)
                .FirstOrDefault(a => a.LibraryAsset.Id == id);
            if (checkout != null) _context.Remove(checkout);

            // close any existing checkout history
            var history = _context.CheckoutHistories
                .Include(h => h.LibraryAsset)
                .Include(h => h.LibraryCard)
                .FirstOrDefault(h =>
                    h.LibraryAsset.Id == id
                    && h.CheckIn == null);
            if (history != null)
            {
                _context.Update(history);
                history.CheckIn = now;
            }

            // look for current holds
            var currentHolds = _context.Holds
                .Include(a => a.LibraryAsset)
                .Include(a => a.LibraryCard)
                .Where(a => a.LibraryAsset.Id == id);

            // if there are current holds, check out the item to the earliest
            if (currentHolds.Any())
            {
                CheckoutToEarliestHold(id, currentHolds);
                return;
            }

            // otherwise, set item status to available
            item.Status = _context.Statuses.FirstOrDefault(a => a.Name == "Available");

            _context.SaveChanges();
        }

        public IEnumerable<CheckoutHistory> GetCheckoutHistory(int id)
        {
            return _context.CheckoutHistories
                .Include(a => a.LibraryAsset)
                .Include(a => a.LibraryCard)
                .Where(a => a.LibraryAsset.Id == id);
        }

        // Remove useless method and replace with finding latest CheckoutHistory if needed 
        public Checkouts GetLatestCheckout(int id)
        {
            return _context.Checkouts
                .Where(c => c.LibraryAsset.Id == id)
                .OrderByDescending(c => c.BorrowDate)
                .FirstOrDefault();
        }

        public int GetNumberOfCopies(int id)
        {
            return _context.LibraryAssets
                .First(a => a.Id == id)
                .NumberOfCopies;
        }

        public bool IsCheckedOut(int id)
        {
            var isCheckedOut = _context.Checkouts.Any(a => a.LibraryAsset.Id == id);

            return isCheckedOut;
        }

        public string GetCurrentHoldPatron(int id)
        {
            var hold = _context.Holds
                .Include(a => a.LibraryAsset)
                .Include(a => a.LibraryCard)
                .Where(v => v.Id == id);

            var cardId = hold
                .Include(a => a.LibraryCard)
                .Select(a => a.LibraryCard.Id)
                .FirstOrDefault();

            var patron = _context.Customers
                .Include(p => p.LibraryCard)
                .First(p => p.LibraryCard.Id == cardId);

            return patron.FirstName + " " + patron.LastName;
        }

        //public string GetCurrentHoldPlaced(int id)
        //{
        //    var hold = _context.Holds
        //        .Include(a => a.LibraryAsset)
        //        .Include(a => a.LibraryCard)
        //        .Where(v => v.Id == id);

        //    return hold.Select(a => a.HoldPlaced)
        //        .FirstOrDefault().ToString(CultureInfo.InvariantCulture);
        //}

        public IEnumerable<Holds> GetCurrentHolds(int id)
        {
            return _context.Holds
                .Include(h => h.LibraryAsset)
                .Where(a => a.LibraryAsset.Id == id);
        }

        public string GetCurrentPatron(int id)
        {
            var checkout = _context.Checkouts
                .Include(a => a.LibraryAsset)
                .Include(a => a.LibraryCard)
                .FirstOrDefault(a => a.LibraryAsset.Id == id);

            if (checkout == null) return "Not checked out.";

            var cardId = checkout.LibraryCard.Id;

            var patron = _context.Customers
                .Include(p => p.LibraryCard)
                .First(c => c.LibraryCard.Id == cardId);

            return patron.FirstName + " " + patron.LastName;
        }

        private void CheckoutToEarliestHold(int assetId, IEnumerable<Holds> currentHolds)
        {
            var earliestHold = currentHolds.OrderBy(a => a.HoldPlaced).FirstOrDefault();
            if (earliestHold == null) return;
            var card = earliestHold.LibraryCard;
            _context.Remove(earliestHold);
            _context.SaveChanges();

            CheckoutItem(assetId, card.Id);
        }

        private DateTime GetDefaultCheckoutTime(DateTime now)
        {
            return now.AddDays(30);
        }

        
       

        

        public string GetCurrentHoldPatronName(int id)
        {
            var hold = _context.Holds
                .Include(a => a.LibraryAsset)
                .Include(a => a.LibraryCard)
                .Where(v => v.Id == id);

            var cardId = hold
                .Include(a => a.LibraryCard)
                .Select(a => a.LibraryCard.Id)
                .FirstOrDefault();

            var patron = _context.Customers
                .Include(p => p.LibraryCard)
                .First(p => p.LibraryCard.Id == cardId);
            string fullName= patron.FirstName + " " + patron.LastName;
            return fullName;
        }

        public string GetCurrentCheckoutPatron(int assetId)
        {
            var checkout = GetCheckoutByAssetId(assetId);

            if(checkout == null)
            {
                return "Not Checked Out";
            }

            var cardId = checkout.LibraryCard.Id;
            var patron = _context.Customers
                .Include(p => p.LibraryCard)
                .FirstOrDefault(o => o.LibraryCard.Id == cardId);
            string fullName = patron.FirstName + " " + patron.LastName;
            return fullName;
             
        }

        private Checkouts GetCheckoutByAssetId(int assetId)
        {
            var checkout = _context.Checkouts
                .Include(j => j.LibraryAsset)
                .Include(k => k.LibraryCard)
                .FirstOrDefault(o => o.LibraryAsset.Id == assetId);
            return checkout;
        }

        public DateTime GetCurrentHoldPlaced(int id)
        {
            return _context.Holds
                .Include(j => j.LibraryAsset)
                .Include(j => j.LibraryCard)
                .FirstOrDefault(k => k.Id == id).HoldPlaced;
        }

        public void CheckOutItem(int assetId, int libraryCardId)
        {
            throw new NotImplementedException();
        }

        
    }
}
