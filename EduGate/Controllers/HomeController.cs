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

        // Combined constructor to inject both dependencies
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var model = new HomePageViewModel();

            // Get the most viewed courses
            model.MostViewedCourses = _context.Courses
                .OrderByDescending(c => c.Views)
                .Take(3)
                .ToList();

            // Get course categories (just an example, you might need to adjust this)
            model.CourseCategories = _context.Courses
                .GroupBy(c => c.Category) // Assuming you have a 'Category' field in your Course model
                .Select(group => new CourseCategory
                {
                    Name = group.Key,
                    Courses = group.ToList()
                })
                .ToList();

            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
