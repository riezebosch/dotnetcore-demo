using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using mvc_demo.Models.HomeViewModels;

namespace mvc_demo.Controllers
{
    public class HomeController : Controller
    {
        public async Task<IActionResult> Index()
        {
            var client = new Services.Client("http://webapi-demo");
            var model = new IndexViewModel();

            // kick off in parallel!
            var values = client.ApiValuesGetAsync();
            var people = client.ApiPeopleGetAsync();

            return View(new IndexViewModel { Values = await values, People = await people });
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
