using System.Diagnostics;
using EduGate.Data;
using EduGate.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EduGate.Controllers
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
            var model = new HomePageViewModel();

            model.MostViewedCourses = _context.Courses
                .OrderByDescending(c => c.Views)
                .Take(3)
                .ToList();

            model.CourseCategories = _context.Courses
                .GroupBy(c => c.Category) 
                .Select(group => new CourseCategory
                {
                    Name = group.Key,
                    Courses = group.ToList()
                })
                .ToList();

            return View(model);
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
