using System;
using System.Collections.Generic;
using System.Text;
using GreaterSydneyLibraries.Models;
using GreaterSydneyLibraries;


namespace LibraryData
{
    public interface ICustomer
    {




        Customers Get(int id);
        IEnumerable<Customers> GetAll();
        void Add(Customers newCustomer);

        IEnumerable<CheckoutHistory> GetCheckoutHistory(int id);
        IEnumerable<Holds> GetHolds(int id);
        IEnumerable<Checkouts> GetCheckouts(int id);










    }
}
