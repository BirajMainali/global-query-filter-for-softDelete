using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SoftDeleteDemo.Data;
using SoftDeleteDemo.Entities;
using SoftDeleteDemo.Models;

namespace SoftDeleteDemo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> New()
        {
            List<DemoModel> testModels = new List<DemoModel>()
            {
                new DemoModel()
                {
                    Description = "Test 1"
                },
                new DemoModel()
                {
                    Description = "Test 2"
                },
                new DemoModel()
                {
                    Description = "test 3"
                }
            };
            foreach (var model in testModels)
            {
                await _context.DemoModel.AddRangeAsync(model);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
    }
}