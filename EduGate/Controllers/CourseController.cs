using Microsoft.AspNetCore.Mvc;
using EduGate.Data;
using EduGate.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authorization;

namespace EduGate.Controllers
{
    public class CourseController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _hostEnvironment;

        public CourseController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _hostEnvironment = hostEnvironment;
        }

        public IActionResult Index()
        {
            // List all courses
            var courses = _context.Courses.ToList();
            return View(courses);
        }
        [HttpPost]
        public async Task<IActionResult> AddReview(int courseId, int stars, string comment)
        {
            // Check if user is logged in
            var studentId = _httpContextAccessor.HttpContext.Session.GetInt32("StudentId");
            if (studentId == null)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Details", "Course", new { id = courseId }) });
            }

            // Get the student and course
            var student = await _context.Students.FindAsync(studentId);
            var course = await _context.Courses.FindAsync(courseId);

            if (student == null || course == null)
            {
                return NotFound();
            }

            // Check if student is enrolled in the course
            var enrollment = await _context.Set<StudentCourse>()
                .FirstOrDefaultAsync(sc => sc.StudentId == studentId && sc.CourseId == courseId);

            if (enrollment == null)
            {
                // Student is not enrolled, cannot review
                TempData["ReviewError"] = "You must be enrolled in this course to leave a review.";
                return RedirectToAction("Details", new { id = courseId });
            }

            // Check if student already reviewed this course
            var existingReview = await _context.Set<CourseReview>()
                .FirstOrDefaultAsync(r => r.StudentId == studentId && r.CourseId == courseId);

            if (existingReview != null)
            {
                // Update existing review
                existingReview.Stars = stars;
                existingReview.Comment = comment;
                existingReview.CreatedDate = DateTime.Now;
            }
            else
            {
                // Create new review
                var review = new CourseReview
                {
                    CourseId = courseId,
                    StudentId = student.Id,
                    Stars = stars,
                    Comment = comment,
                    CreatedDate = DateTime.Now
                };

                _context.Add(review);
            }

            await _context.SaveChangesAsync();
            TempData["ReviewSuccess"] = "Thank you for your review!";
            return RedirectToAction("Details", new { id = courseId });
        }

        // Let's also update the Details action to include the review-related data
        // and the actual course modules and related courses
        // Replace or modify the existing Details action in CourseController.cs
        public async Task<IActionResult> Details(int id)
        {
            // Get course with related data
            var course = await _context.Courses
                .Include(c => c.Teacher)
                .Include(c => c.Modules)
                    .ThenInclude(m => m.Contents)
                .Include(c => c.Reviews)
                    .ThenInclude(r => r.Student)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            // Increment view count
            course.Views++;
            await _context.SaveChangesAsync();

            // Find related courses (same category)
            var relatedCourses = await _context.Courses
                .Where(c => c.Category == course.Category && c.Id != course.Id)
                .OrderByDescending(c => c.Views)
                .Take(3)
                .ToListAsync();

            // Check if student is enrolled
            var studentId = _httpContextAccessor.HttpContext.Session.GetInt32("StudentId");
            bool isEnrolled = false;
            bool hasReviewed = false;

            if (studentId.HasValue)
            {
                var student = await _context.Students
                    .Include(s => s.EnrolledCourses)
                    .FirstOrDefaultAsync(s => s.Id == studentId);

                if (student != null)
                {
                    ViewBag.CurrentStudent = student;

                    // Check if student is enrolled in the course
                    isEnrolled = student.EnrolledCourses.Any(c => c.CourseId == course.Id);

                    // Check if student has already reviewed this course
                    hasReviewed = await _context.Set<CourseReview>()
                        .AnyAsync(r => r.StudentId == studentId && r.CourseId == course.Id);
                }
            }

            // Create view model
            var viewModel = new CourseDetailsViewModel
            {
                Course = course,
                RelatedCourses = relatedCourses,
                IsEnrolled = isEnrolled,
                HasReviewed = hasReviewed
            };

            return View(viewModel);
        }


        [HttpPost]
        public IActionResult Enroll(int id)
        {
            // Check if user is logged in
            var studentId = _httpContextAccessor.HttpContext.Session.GetInt32("StudentId");
            if (studentId == null)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Details", "Course", new { id = id }) });
            }

            var student = _context.Students.Include(s => s.EnrolledCourses).FirstOrDefault(s => s.Id == studentId);
            var course = _context.Courses.FirstOrDefault(c => c.Id == id);

            if (student == null || course == null)
            {
                return NotFound();
            }

            // Enroll only if not already enrolled
            if (!student.EnrolledCourses.Any(sc => sc.CourseId == course.Id))
            {
                var enrollment = new StudentCourse
                {
                    StudentId = student.Id,
                    CourseId = course.Id,
                    EnrollmentDate = DateTime.Now,
                    IsCompleted = false
                };

                student.EnrolledCourses.Add(enrollment);
                _context.SaveChanges();
            }

            return RedirectToAction("Learn", "Course", new { id = id });
        }


        public IActionResult Learn(int id, int? moduleId = null, int? contentId = null)
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

            // If there are no modules, display a message and skip processing modules/contents
            if (course.Modules == null || !course.Modules.Any())
            {
                // No modules, just show the course with no content
                var viewModel2 = new CourseLearnViewModel
                {
                    Course = course,
                    StudentProgress = new Dictionary<int, bool>(),
                    ContentAttempts = new Dictionary<int, int>(),
                    ContentScores = new Dictionary<int, int>()
                };

                // Return the learning view with a course but no modules
                ViewBag.NoModulesMessage = "This course currently has no modules or content available.";
                return View(viewModel2);
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

            if (student == null || !student.EnrolledCourses.Any(ec => ec.Id == course.Id))
            {
                return RedirectToAction("Details", "Course", new { id = id });
            }

            // Create view model with course and progress data
            var viewModel = new CourseLearnViewModel
            {
                Course = course,
                StudentProgress = new Dictionary<int, bool>(),
                ContentAttempts = new Dictionary<int, int>(),
                ContentScores = new Dictionary<int, int>()
            };

            // Add progress information
            foreach (var module in course.Modules)
            {
                foreach (var content in module.Contents)
                {
                    var progress = student.ContentProgresses.FirstOrDefault(cp => cp.ContentId == content.Id);
                    viewModel.StudentProgress[content.Id] = progress?.IsCompleted ?? false;

                    if (progress != null)
                    {
                        viewModel.ContentAttempts[content.Id] = progress.Attempts;
                        if (progress.Score.HasValue)
                        {
                            viewModel.ContentScores[content.Id] = progress.Score.Value;
                        }
                    }
                }
            }

            // Return the learning view with course data
            return View(viewModel);
        }

        [HttpPost]
        [HttpPost]
        public IActionResult MarkContentComplete(int id, int contentId, string returnUrl = null)
        {
            var studentId = _httpContextAccessor.HttpContext.Session.GetInt32("StudentId");
            if (studentId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var student = _context.Students
                .Include(s => s.ContentProgresses)
                .Include(s => s.EnrolledCourses)
                .FirstOrDefault(s => s.Id == studentId);

            var content = _context.Set<ModuleContent>()
                .Include(c => c.Module)
                .ThenInclude(m => m.Course)
                .FirstOrDefault(c => c.Id == contentId);

            if (student == null || content == null)
            {
                return NotFound();
            }

            var progress = student.ContentProgresses.FirstOrDefault(cp => cp.ContentId == contentId);

            if (progress == null)
            {
                progress = new ContentProgress
                {
                    StudentId = student.Id,
                    ContentId = contentId,
                    IsCompleted = true,
                    CompletedDate = DateTime.Now
                };
                student.ContentProgresses.Add(progress);
            }
            else
            {
                progress.IsCompleted = true;
                progress.CompletedDate = DateTime.Now;
            }

            _context.SaveChanges();

            // 🔍 Check if all contents in the course are marked as completed by this student
            var courseId = content.Module.CourseId;

            var courseContents = _context.ModuleContents
                .Include(c => c.Module)
                .Where(c => c.Module.CourseId == courseId)
                .Select(c => c.Id)
                .ToList();

            var completedContentIds = student.ContentProgresses
                .Where(cp => cp.IsCompleted)
                .Select(cp => cp.ContentId)
                .ToList();

            bool allCompleted = courseContents.All(cid => completedContentIds.Contains(cid));

            if (allCompleted)
            {
                var studentCourse = student.EnrolledCourses
                    .FirstOrDefault(sc => sc.CourseId == courseId);

                if (studentCourse != null && !studentCourse.IsCompleted)
                {
                    studentCourse.IsCompleted = true;
                    studentCourse.CompletionDate = DateTime.Now;
                    _context.SaveChanges(); // 💾 Save course completion status
                }
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Learn", new { id = id });
        }


        [HttpPost]
        public IActionResult SubmitQuiz(int id, int quizId, List<int> Answers, string returnUrl = null)
        {
            // Check if user is logged in
            var studentId = _httpContextAccessor.HttpContext.Session.GetInt32("StudentId");
            if (studentId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Find the student, quiz, and questions
            var student = _context.Students
                .Include(s => s.ContentProgresses)
                .FirstOrDefault(s => s.Id == studentId);

            var quizContent = _context.Set<QuizContent>()
                .FirstOrDefault(q => q.Id == quizId);

            if (student == null || quizContent == null)
            {
                return NotFound();
            }

            // Get quiz questions
            var questions = _context.Set<QuizQuestion>()
                .Where(q => q.QuizContentId == quizId)
                .ToList();

            if (!questions.Any() || Answers == null || Answers.Count != questions.Count)
            {
                return BadRequest();
            }

            // Calculate score
            int totalPoints = questions.Sum(q => q.Points);
            int earnedPoints = 0;

            for (int i = 0; i < questions.Count; i++)
            {
                if (i < Answers.Count && Answers[i] == questions[i].CorrectOptionIndex)
                {
                    earnedPoints += questions[i].Points;
                }
            }

            int scorePercentage = (int)Math.Round((double)earnedPoints / totalPoints * 100);

            // Update student progress
            var progress = student.ContentProgresses.FirstOrDefault(cp => cp.ContentId == quizId);

            if (progress == null)
            {
                // Create new progress record
                progress = new ContentProgress
                {
                    StudentId = student.Id,
                    ContentId = quizId,
                    Attempts = 1,
                    Score = scorePercentage,
                    IsCompleted = scorePercentage >= quizContent.PassingScore,
                    CompletedDate = scorePercentage >= quizContent.PassingScore ? DateTime.Now : null
                };

                student.ContentProgresses.Add(progress);
            }
            else
            {
                // Update existing progress
                progress.Attempts++;
                progress.Score = scorePercentage;

                if (!progress.IsCompleted && scorePercentage >= quizContent.PassingScore)
                {
                    progress.IsCompleted = true;
                    progress.CompletedDate = DateTime.Now;
                }
            }

            _context.SaveChanges();

            // Set temp data for quiz results
            TempData["QuizScore"] = scorePercentage;
            TempData["QuizPassed"] = scorePercentage >= quizContent.PassingScore;

            // Redirect back to the calling page if provided
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // Otherwise, redirect to the Learn page
            return RedirectToAction("Learn", new { id = id });
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
                    thumbnailUrl = c.ImageUrl.Replace("~/", ""), // Using ImageUrl from your model
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

        #region Teacher section part
        // GET: Course/Create
        public IActionResult Create()
        {
            return View(new CourseCreateViewModel());
        }

        // POST: Course/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CourseCreateViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Get current teacher
                    var teacherId = HttpContext.Session.GetInt32("TeacherId");
                    if (!teacherId.HasValue)
                    {
                        return Unauthorized();
                    }

                    var teacher = await _context.Teachers.FindAsync(teacherId);
                    if (teacher == null)
                    {
                        return NotFound();
                    }

                    // Handle image upload
                    string uniqueFileName = null;
                    if (model.ImageFile != null)
                    {
                        string courseImagesFolder = Path.Combine(_hostEnvironment.WebRootPath, "img", "courses");
                        if (!Directory.Exists(courseImagesFolder))
                        {
                            Directory.CreateDirectory(courseImagesFolder);
                        }

                        uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.ImageFile.FileName);
                        string filePath = Path.Combine(courseImagesFolder, uniqueFileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.ImageFile.CopyToAsync(fileStream);
                        }
                    }

                    // Create course
                    var course = new Course
                    {
                        Title = model.Title,
                        Category = model.Category,
                        Description = model.Description,
                        ImageUrl = uniqueFileName != null ? "~/img/courses/" + uniqueFileName : "",
                        TeacherId = teacher.Id,
                        Views = 0
                    };

                    // Add course to database
                    _context.Courses.Add(course);
                    await _context.SaveChangesAsync();

                    // Redirect to module creation page
                    return RedirectToAction("CreateModule", new { courseId = course.Id });
                }

                return View(model);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine(ex.Message);
                ModelState.AddModelError("", "An error occurred while creating the course. Please try again.");
                return View(model);
            }
        }

        // GET: Course/CreateModule
        public async Task<IActionResult> CreateModule(int courseId)
        {
            var course = await _context.Courses
                .Include(c => c.Modules)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
            {
                return NotFound();
            }

            var viewModel = new ModuleCreateViewModel
            {
                CourseId = courseId,
                CourseTitle = course.Title,
                ExistingModules = course.Modules.OrderBy(m => m.Order).ToList()
            };

            return View(viewModel);
        }

        // POST: Course/CreateModule
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateModule(ModuleCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var course = await _context.Courses.FindAsync(model.CourseId);
                if (course == null)
                {
                    return NotFound();
                }

                // Get current teacher
                var teacherId = HttpContext.Session.GetInt32("TeacherId");
                if (course.TeacherId != teacherId)
                {
                    return Unauthorized();
                }

                var teacher = await _context.Teachers.FindAsync(teacherId);

                // Create module
                var module = new Module
                {
                    Id = _context.Modules.ToList().Count,
                    Title = model.Title,
                    Description = model.Description,
                    CourseId = course.Id,
                    Order = course.Modules.Count + 1 // or use _context.Modules.Count for course
                };

                _context.Modules.Add(module);

                await _context.SaveChangesAsync();

                // Redirect to content creation page
                return RedirectToAction("CreateContent", new { moduleId = module.Id });
            }

            // If we got this far, something failed, redisplay form
            var courseForRedisplay = await _context.Courses
                .Include(c => c.Modules)
                .FirstOrDefaultAsync(c => c.Id == model.CourseId);

            model.CourseTitle = courseForRedisplay.Title;
            model.ExistingModules = courseForRedisplay.Modules.OrderBy(m => m.Order).ToList();

            return View(model);
        }

        // GET: Course/CreateContent
        public async Task<IActionResult> CreateContent(int moduleId)
        {
            var module = await _context.Modules
                .Include(m => m.Course)
                .Include(m => m.Contents)
                .FirstOrDefaultAsync(m => m.Id == moduleId);

            if (module == null)
            {
                return NotFound();
            }

            var viewModel = new ContentCreateViewModel
            {
                ModuleId = moduleId,
                ModuleTitle = module.Title,
                CourseId = module.CourseId,
                CourseTitle = module.Course.Title,
                ExistingContents = module.Contents.OrderBy(c => c.Order).ToList()
            };

            return View(viewModel);
        }

        // POST: Course/CreateContent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateContent(ContentCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var module = await _context.Modules
                    .Include(m => m.Course)
                    .FirstOrDefaultAsync(m => m.Id == model.ModuleId);

                if (module == null)
                {
                    return NotFound();
                }

                // Get current teacher
                var teacherId = HttpContext.Session.GetInt32("TeacherId");
                if (module.Course.TeacherId != teacherId)
                {
                    return Unauthorized();
                }

                var teacher = await _context.Teachers.FindAsync(teacherId);

                // Create content based on type
                ModuleContent content = null;
                string courseFolder = module.Course.Title.Replace(" ", "");
                string contentFolder = Path.Combine(_hostEnvironment.WebRootPath, "contents");

                switch (model.ContentType)
                {
                    case "Video":
                        if (model.VideoFile != null)
                        {
                            string videoFolder = Path.Combine(contentFolder, "vid", courseFolder);
                            if (!Directory.Exists(videoFolder))
                            {
                                Directory.CreateDirectory(videoFolder);
                            }

                            string videoFileName = model.Title.Replace(" ", "") + Path.GetExtension(model.VideoFile.FileName);
                            string videoPath = Path.Combine(videoFolder, videoFileName);

                            using (var fileStream = new FileStream(videoPath, FileMode.Create))
                            {
                                await model.VideoFile.CopyToAsync(fileStream);
                            }

                            var videoContent = new VideoContent
                            {
                                Title = model.Title,
                                ShortDescription = model.ShortDescription,
                                Order = module.Contents.Count + 1,
                                VideoLocation = $"~/contents/vid/{courseFolder}/{videoFileName}",
                                DurationSeconds = model.DurationSeconds
                            };

                            teacher.AddVideoToModule(module, videoContent);
                            content = videoContent;
                            _context.VideoContents.Add(videoContent);
                            _context.SaveChanges();
                        }
                        break;

                    case "Text":
                        if (model.TextFile != null)
                        {
                            string textFolder = Path.Combine(contentFolder, "txt", courseFolder);
                            if (!Directory.Exists(textFolder))
                            {
                                Directory.CreateDirectory(textFolder);
                            }

                            string textFileName = model.Title.Replace(" ", "") + Path.GetExtension(model.TextFile.FileName);
                            string textPath = Path.Combine(textFolder, textFileName);

                            using (var fileStream = new FileStream(textPath, FileMode.Create))
                            {
                                await model.TextFile.CopyToAsync(fileStream);
                            }

                            var textContent = new TextContent
                            {
                                Title = model.Title,
                                ShortDescription = model.ShortDescription,
                                Order = module.Contents.Count + 1,
                                TextLocation = $"~/contents/txt/{courseFolder}/{textFileName}"
                            };

                            teacher.AddTextContentToModule(module, textContent);
                            content = textContent;
                            _context.TextContents.Add(textContent);
                            _context.SaveChanges();
                        }
                        break;

                    case "Quiz":
                        var quizContent = new QuizContent
                        {
                            Title = model.Title,
                            ShortDescription = model.ShortDescription,
                            Order = module.Contents.Count + 1,
                            PassingScore = model.PassingScore ?? 70,
                            MaxAttempts = model.MaxAttempts
                        };

                        teacher.AddQuizToModule(module, quizContent);
                        content = quizContent;
                        _context.Quizzes.Add(quizContent);
                        await _context.SaveChangesAsync();
                        // After saving, redirect to create quiz questions
                        return RedirectToAction("CreateQuizQuestions", new { quizContentId = quizContent.Id });
                        break;
                }

                if (content != null)
                {
                    await _context.SaveChangesAsync();
                    // Redirect back to content creation for the same module
                    return RedirectToAction("CreateContent", new { moduleId = module.Id });
                }
            }

            // If we got this far, something failed, redisplay form
            var moduleForRedisplay = await _context.Modules
                .Include(m => m.Course)
                .Include(m => m.Contents)
                .FirstOrDefaultAsync(m => m.Id == model.ModuleId);

            model.ModuleTitle = moduleForRedisplay.Title;
            model.CourseTitle = moduleForRedisplay.Course.Title;
            model.ExistingContents = moduleForRedisplay.Contents.OrderBy(c => c.Order).ToList();

            return View(model);
        }

        // GET: Course/CreateQuizQuestions
        public async Task<IActionResult> CreateQuizQuestions(int quizContentId)
        {
            var quizContent = await _context.Set<QuizContent>()
                .Include(q => q.Module)
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == quizContentId);

            if (quizContent == null)
            {
                return NotFound();
            }

            var viewModel = new QuizQuestionsViewModel
            {
                QuizContentId = quizContentId,
                QuizTitle = quizContent.Title,
                ModuleId = quizContent.ModuleId,
                ExistingQuestions = quizContent.Questions.ToList()
            };

            return View(viewModel);
        }

        // POST: Course/CreateQuizQuestions
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateQuizQuestions(QuizQuestionsViewModel model)
        {
            if (ModelState.IsValid)
            {
                var quizContent = await _context.Set<QuizContent>()
                    .Include(q => q.Module)
                        .ThenInclude(m => m.Course)
                    .FirstOrDefaultAsync(q => q.Id == model.QuizContentId);

                if (quizContent == null)
                {
                    return NotFound();
                }

                // Get current teacher
                var teacherId = HttpContext.Session.GetInt32("TeacherId");
                if (quizContent.Module.Course.TeacherId != teacherId)
                {
                    return Unauthorized();
                }

                // Create quiz question
                var question = new QuizQuestion
                {
                    Question = model.Question,
                    Options = new List<string> { model.Option1, model.Option2, model.Option3, model.Option4 },
                    CorrectOptionIndex = model.CorrectOptionIndex,
                    Points = model.Points,
                    QuizContentId = quizContent.Id
                };

                quizContent.Questions.Add(question);
                _context.QuizQuestions.Add(question);
                await _context.SaveChangesAsync();

                // Redirect back to quiz questions page (add another question)
                return RedirectToAction("CreateQuizQuestions", new { quizContentId = quizContent.Id });
            }

            // If we got this far, something failed, redisplay form
            var quizContentForRedisplay = await _context.Set<QuizContent>()
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == model.QuizContentId);

            model.QuizTitle = quizContentForRedisplay.Title;
            model.ExistingQuestions = quizContentForRedisplay.Questions.ToList();

            return View(model);
        }

        // GET: Course/CompleteQuiz
        public async Task<IActionResult> CompleteQuiz(int quizContentId)
        {
            var quizContent = await _context.Set<QuizContent>()
                .Include(q => q.Module)
                .FirstOrDefaultAsync(q => q.Id == quizContentId);

            if (quizContent == null)
            {
                return NotFound();
            }

            // Redirect back to module content page
            return RedirectToAction("CreateContent", new { moduleId = quizContent.ModuleId });
        }
    }

    #endregion Teacher section part

}