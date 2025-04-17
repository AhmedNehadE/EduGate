using Microsoft.AspNetCore.Mvc;
using EduGate.Data;
using EduGate.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EduGate.Controllers
{
    public class CourseController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CourseController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult Index()
        {
            // List all courses
            var courses = _context.Courses.ToList();
            return View(courses);
        }

        public IActionResult Details(int id)
        {
            // Find the course by ID
            var course = _context.Courses.FirstOrDefault(c => c.Id == id);

            // If course not found, return NotFound
            if (course == null)
            {
                return NotFound();
            }

            // Increment the view count when accessing details
            course.Views++;
            _context.SaveChanges();

            // Get current student information if logged in
            var studentId = _httpContextAccessor.HttpContext.Session.GetInt32("StudentId");
            if (studentId.HasValue)
            {
                var student = _context.Students
                    .Include(s => s.EnrolledCourses)
                    .FirstOrDefault(s => s.Id == studentId);

                if (student != null)
                {
                    ViewBag.CurrentStudent = student;
                }
            }

            // Return the course details view with the course model
            return View(course);
        }

        [HttpPost]
        public IActionResult Enroll(int id)
        {
            // Check if user is logged in
            var studentId = _httpContextAccessor.HttpContext.Session.GetInt32("StudentId");
            if (studentId == null)
            {
                // Redirect to login if not logged in, with returnUrl to come back to this course
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Details", "Course", new { id = id }) });
            }

            // Find the student and course
            var student = _context.Students.Include(s => s.EnrolledCourses).FirstOrDefault(s => s.Id == studentId);
            var course = _context.Courses.FirstOrDefault(c => c.Id == id);

            if (student == null || course == null)
            {
                return NotFound();
            }

            // Enroll student if not already enrolled
            if (!student.EnrolledCourses.Any(c => c.Id == course.Id))
            {
                student.EnrollInCourse(course);
                _context.SaveChanges();
            }

            // Redirect to course details or a learning page
            return RedirectToAction("Learn", "Course", new { id = id });
        }

        public IActionResult Learn(int id)
        {
            // Check if user is logged in
            var studentId = _httpContextAccessor.HttpContext.Session.GetInt32("StudentId");
            if (studentId == null)
            {
                // Redirect to login
                return RedirectToAction("Login", "Account");
            }

            // First, load the course with modules and all contents
            var course = _context.Courses
                .Include(c => c.Modules)
                .ThenInclude(m => m.Contents)
                .FirstOrDefault(c => c.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            // Then separately load quiz questions
            var quizContentIds = course.Modules
                .SelectMany(m => m.Contents)
                .Where(c => c is QuizContent)
                .Select(c => c.Id)
                .ToList();

            if (quizContentIds.Any())
            {
                var quizQuestions = _context.Set<QuizQuestion>()
                    .Where(q => quizContentIds.Contains(q.QuizContentId))
                    .ToList();

                // Manually associate questions with their quiz contents
                foreach (var module in course.Modules)
                {
                    foreach (var content in module.Contents)
                    {
                        if (content is QuizContent quizContent)
                        {
                            quizContent.Questions = quizQuestions
                                .Where(q => q.QuizContentId == content.Id)
                                .ToList();
                        }
                    }
                }
            }

            // Check if student is enrolled
            var student = _context.Students
                .Include(s => s.EnrolledCourses)
                .Include(s => s.ContentProgresses)
                .FirstOrDefault(s => s.Id == studentId);

            if (student == null || !student.EnrolledCourses.Any(ec => ec.CourseId == course.Id))
            {
                return RedirectToAction("Details", "Course", new { id = id });
            }

            // Create view model with course and progress data
            var viewModel = new CourseLearnViewModel
            {
                Course = course,
                StudentProgress = new Dictionary<int, bool>()
            };

            // Add progress information
            foreach (var module in course.Modules)
            {
                foreach (var content in module.Contents)
                {
                    var progress = student.ContentProgresses.FirstOrDefault(cp => cp.ContentId == content.Id);
                    viewModel.StudentProgress[content.Id] = progress?.IsCompleted ?? false;
                }
            }

            // Return the learning view with course data
            return View(viewModel);
        }
        // NEW CODE FOR SEARCH FUNCTIONALITY

        /// <summary>
        /// Returns course suggestions for the search dropdown (max 5)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SearchSuggestions(string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            {
                return Json(new List<object>());
            }

            // Normalize query
            query = query.Trim().ToLower();
            var searchTerms = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // Query courses with any matching terms in title, description, or category
            var courses = await _context.Courses
                .Include(c => c.Teacher)
                .Where(c =>
                    searchTerms.Any(term =>
                        c.Title.ToLower().Contains(term) ||
                        c.Description.ToLower().Contains(term) ||
                        c.Category.ToLower().Contains(term))
                )
                .Select(c => new
                {
                    id = c.Id,
                    title = c.Title,
                    description = c.Description,
                    instructorName = c.Teacher.Name, // Assuming Teacher has FirstName and LastName
                    category = c.Category,
                    thumbnailUrl = c.ImageUrl, // Using ImageUrl from your model
                    // Calculate relevance score: number of matching terms in title and description
                    relevanceScore = searchTerms.Count(term =>
                        c.Title.ToLower().Contains(term)) * 3 + // Title matches weighted more
                        searchTerms.Count(term => c.Description.ToLower().Contains(term)) +
                        searchTerms.Count(term => c.Category.ToLower().Contains(term))
                })
                .OrderByDescending(c => c.relevanceScore) // Order by relevance
                .Take(5) // Take only top 5
                .ToListAsync();

            return Json(courses);
        }

        /// <summary>
        /// Full search results page
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Search(string query, int page = 1)
        {
            const int pageSize = 10;

            if (string.IsNullOrWhiteSpace(query))
            {
                return RedirectToAction("Index", "Home");
            }

            // Normalize query
            query = query.Trim().ToLower();
            var searchTerms = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // Find all matching courses
            var coursesQuery = _context.Courses
                .Include(c => c.Teacher)
                .Where(c =>
                    searchTerms.Any(term =>
                        c.Title.ToLower().Contains(term) ||
                        c.Description.ToLower().Contains(term) ||
                        c.Category.ToLower().Contains(term))
                )
                .Select(c => new CourseSearchViewModel
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    TeacherName = c.Teacher.Name, // Assuming Teacher has FirstName and LastName
                    Category = c.Category,
                    ImageUrl = c.ImageUrl,
                    Views = c.Views,
                    // Calculate relevance score based on matching terms
                    RelevanceScore = searchTerms.Count(term =>
                        c.Title.ToLower().Contains(term)) * 3 + // Title matches weighted more
                        searchTerms.Count(term => c.Description.ToLower().Contains(term)) +
                        searchTerms.Count(term => c.Category.ToLower().Contains(term))
                })
                .OrderByDescending(c => c.RelevanceScore); // Order by relevance

            // Count total results for pagination
            int totalItems = await coursesQuery.CountAsync();

            // Get paginated results
            var courses = await coursesQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Create view model with pagination info
            var viewModel = new CourseSearchResultsViewModel
            {
                SearchQuery = query,
                Courses = courses,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                TotalResults = totalItems
            };

            return View(viewModel);
        }
    }

    
}