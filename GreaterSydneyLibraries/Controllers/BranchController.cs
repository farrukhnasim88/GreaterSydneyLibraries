using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LibraryData;
using GreaterSydneyLibraries.Models.Branch;

namespace GreaterSydneyLibraries.Controllers
{
    public class BranchController : Controller
    {

        private readonly ILibraryBranch _branchService;

        public BranchController(ILibraryBranch branchService)
        {
            _branchService = branchService;
        }

        public IActionResult Index()
        {
            var branchModels = _branchService.GetAll()
                .Select(br => new BranchDetailModel
                {
                    Id = br.Id,
                    BranchName = br.Name,
                    NumberOfAssets = _branchService.GetAssetCount(br.Id),
                    NumberOfPatrons = _branchService.GetPatronCount(br.Id),
                    IsOpen = _branchService.IsBranchOpen(br.Id)
                }).ToList();

            var model = new BranchIndexModel
            {
                Branches = branchModels
            };

            return View(model);
        }

        public IActionResult Detail(int id)
        {
            var branch = _branchService.Get(id);
            var model = new BranchDetailModel
            {
                BranchName = branch.Name,
                Description = branch.Description,
                Address = branch.Address,
                Telephone = branch.Telephone,
                BranchOpenedDate = branch.OpneDate.ToString("yyyy-MM-dd"),
                NumberOfPatrons = _branchService.GetPatronCount(id),
                NumberOfAssets = _branchService.GetAssetCount(id),
                TotalAssetValue = _branchService.GetAssetsValue(id),
                ImageUrl = branch.ImageUrl,
                HoursOpen = _branchService.GetBranchHours(id)
            };

            return View(model);
        }
    }
}