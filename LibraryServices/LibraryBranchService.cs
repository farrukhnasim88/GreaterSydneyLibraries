using GreaterSydneyLibraries.Models;
using LibraryData;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibraryServices
{
    class LibraryBranchService : ILibraryBranch
    {
        private readonly LibraryContext _context;

        public LibraryBranchService(LibraryContext context)
        {
            _context = context;
        }

        public void Add(LibraryBranch newBranch)
        {
            _context.Add(newBranch);
            _context.SaveChanges();
        }

        public LibraryBranch Get(int branchId)
        {
            return _context.LibraryBranches
                .Include(b => b.Customers)
                .Include(b => b.GetLibraryAssets)
                .FirstOrDefault(p => p.Id == branchId);
        }

        public IEnumerable<LibraryBranch> GetAll()
        {
            return _context.LibraryBranches.Include(a => a.Customers).Include(a => a.GetLibraryAssets);
        }

        public int GetAssetCount(int branchId)
        {
            return Get(branchId).GetLibraryAssets.Count();
        }

        public IEnumerable<LibraryAsset> GetAssets(int branchId)
        {
            return _context.LibraryBranches.Include(a => a.GetLibraryAssets)
                .First(b => b.Id == branchId).GetLibraryAssets;
        }

        public decimal GetAssetsValue(int branchId)
        {
            var assetsValue = GetAssets(branchId).Select(a => a.Cost);
            return assetsValue.Sum();
        }

        public IEnumerable<string> GetBranchHours(int branchId)
        {
            var hours = _context.BranchHours.Where(a => a.Branch.Id == branchId);

            var displayHours =
                DataHelpers.HumanizeBusinessHours(hours);

            return displayHours;
        }

        public int GetPatronCount(int branchId)
        {
            return Get(branchId).Customers.Count();
        }

        public IEnumerable<Customers> GetPatrons(int branchId)
        {
            return _context.LibraryBranches.Include(a => a.Customers).First(b => b.Id == branchId).Customers;
        }

         
        public bool IsBranchOpen(int branchId)
        {
            var currentTimeHour = DateTime.Now.Hour;
            var CurrentDayOfWeek = (int)DateTime.Now.DayOfWeek + 1;
            var hours = _context.BranchHours.Where(h => h.Branch.Id == branchId);
            var dayHours = hours.FirstOrDefault(h => h.DayOfWeek == CurrentDayOfWeek);

            return currentTimeHour < dayHours.CloseTime && currentTimeHour > dayHours.OpenTime;
        }
    }
}
