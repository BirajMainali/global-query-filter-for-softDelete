using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoftDeleteDemo.Data;
using SoftDeleteDemo.Entities;

namespace SoftDeleteDemo.Controllers
{
    public class DemoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DemoController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var demos = await _context.DemoModel.ToListAsync();
            return View(demos);
        }
        
        public async Task<IActionResult> New()
        {
            var demos = new List<DemoModel>()
            {
                new DemoModel()
                {
                    Description = "Soft Model Demo 1",
                },
                new DemoModel()
                {
                    Description = "Soft Model Demo 2",
                },
                new DemoModel()
                {
                    Description = "Soft Model Demo 3"
                }
            };
            foreach (var demo in demos)
            {
                await _context.DemoModel.AddRangeAsync(demo);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
        
        public async Task<IActionResult> Remove(Guid id)
        {
            var demo = await _context.DemoModel.FindAsync(id);

            _context.DemoModel.Remove(demo);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}