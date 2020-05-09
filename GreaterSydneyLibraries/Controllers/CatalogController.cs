using Microsoft.AspNetCore.Mvc;
using System.Linq;
using GreaterSydneyLibraries.Models;
using GreaterSydneyLibraries.Models.Catalog;
using LibraryData;
using GreaterSydneyLibraries.Models.Checkout;

namespace GreaterSydneyLibraries.Controllers
{
    public class CatalogController: Controller
    {
        private readonly ILibraryAsset _assets;
        private readonly ICheckout _checkouts;

        public CatalogController(ILibraryAsset assets, ICheckout checkout)
        {
            _assets = assets;
            _checkouts= checkout;

        }

        public IActionResult Indexes()
        {
            return View();
        }

        public IActionResult Index()
        {
            var assetModels = _assets.GetAll();
            var listingResult = assetModels
                .Select(result => new AssetIndexListingModel
                {

                    Id = result.Id,
                    ImageUrl = result.ImageUrl,
                    AuthorOrDirector = _assets.GetAuthorOrDirector(result.Id),
                    DeweyCallNumber = _assets.GetDeweyIndex(result.Id),
                    Title = result.Title,
                    Type = _assets.GetType(result.Id)


                });
            var model = new AssetIndexModel()
            {

                Assets = listingResult
            };

            return View(model);

        }

        public IActionResult Detail(int id)
        {
            var asset = _assets.GetById(id);
            var currentHolds = _checkouts.GetCurrentHolds(id)
                              .Select(a => new AssetHoldModel
                              {
                                  HoldPlaced= _checkouts.GetCurrentHoldPlaced(a.Id).ToString("d"),
                                  CustomerName= _checkouts.GetCurrentHoldPatronName(a.Id)
                              });

            var model = new AssetDetailModel
            {
                AssetId = id,
                Title = asset.Title,
                Year = asset.Year,
                Cost = asset.Cost,
                Status = asset.Status.Name,
                ImageUrl = asset.ImageUrl,
                AuthorOrDirector = _assets.GetAuthorOrDirector(id),
                CurrentLocation = _assets.GetCurrentLocation(id).Name,  //navigation property
                DeweyCallNumber = _assets.GetDeweyIndex(id),
                ISBN = _assets.GetIsbn(id),
                CheckoutHistories = _checkouts.GetCheckoutHistory(id),
                LatestCheckout = _checkouts.GetLatestCheckout(id),
                CustomerName= _checkouts.GetCurrentCheckoutPatron(id),
                CurrentHold= currentHolds

            
                
            };
            return View(model);


        }


        public IActionResult Checkout(int id)
        {
            var asset = _assets.GetById(id);
            var model = new CheckoutModel {
            
            AssetId= id,
            ImageUrl= asset.ImageUrl,
            Title= asset.Title,
            LibraryCardId= "",
            IsCheckdOut= _checkouts.IsCheckedOut(id)
            
            
            };

            return View(model);
        }


        public IActionResult MarkLost(int assetId)
        {
            _checkouts.MarkLost(assetId);
            return RedirectToAction("Detail", new { id = assetId });
        }
        public IActionResult MarkFound(int assetId)
        {
            _checkouts.MarkFound(assetId);
            return RedirectToAction("Detail", new { id = assetId });
        }

        public IActionResult Hold(int id)
        {
            var asset = _assets.GetById(id);
            var model = new CheckoutModel
            {
                AssetId = id,
                ImageUrl = asset.ImageUrl,
                Title = asset.Title,
                LibraryCardId = "",
                IsCheckdOut = _checkouts.IsCheckedOut(id),
                HoldCount= _checkouts.GetCurrentHolds(id).Count()

            };

            return View(model);
        }


        [HttpPost]
        public IActionResult PlaceCheckout(int assetId, int libraryCardId)
        {
            _checkouts.CheckOutItem(assetId, libraryCardId);
            return RedirectToAction("Detail", new { id = assetId });
        }

        public IActionResult CheckIn(int id)
        {
            _checkouts.CheckInItem(id);
            return RedirectToAction("Detail", new { id = id });
        }


        [HttpPost]
        public IActionResult PlaceHold (int assetId, int libraryCardId)
        {
            _checkouts.PlaceHold(assetId, libraryCardId);
            return RedirectToAction("Detail", new { id = assetId });
        }
    }
}
