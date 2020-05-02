using GreaterSydneyLibraries.Models;
using System;
using System.Collections.Generic;

namespace LibraryData
{
    public interface ICheckout
    {
        IEnumerable<Checkouts> GetAll();
        IEnumerable<CheckoutHistory> GetCheckoutHistory(int id);
        IEnumerable<Holds> GetCurrentHolds(int id);



        Checkouts GetById(int checkoutId);
        Checkouts GetLatestCheckout(int assetId);
        string GetCurrentCheckoutPatron(int assetId);
        string GetCurrentHoldPatronName(int id);
        DateTime GetCurrentHoldPlaced(int id);
        bool IsCheckedOut(int id);

        void Add(Checkouts newCheckout);
        void CheckOutItem(int assetId, int libraryCardId);
        void CheckInItem(int assetId, int libraryCardId);
        void PlaceHold(int assetId, int libraryCardId);
        void MarkLost(int assetId);
        void MarkFound(int assetId);


    }
}
