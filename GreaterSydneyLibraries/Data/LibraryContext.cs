using Microsoft.EntityFrameworkCore;
using System;
using GreaterSydneyLibraries.Models;
namespace LibraryData
{
    public class LibraryContext: DbContext
    {

        public LibraryContext( DbContextOptions options): base(options) 
        {

        }

        public DbSet<Customers> Customers { get; set; }

    }
}
