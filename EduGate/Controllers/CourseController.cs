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
                _context.StudentCourses.Add(enrollment);
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

            if (student == null || !student.EnrolledCourses.Any(ec => ec.CourseId == course.Id))
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

#region Teacher create section part
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
                    teacher.UploadedCourses.Add(course);
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
            var course = await _context.Courses
            .Include(c => c.Modules)
            .FirstOrDefaultAsync(c => c.Id == model.CourseId);
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
                Title = model.Title,
                Description = model.Description,
                CourseId = course.Id,
                Order = course.Modules.Count + 1 // or use _context.Modules.Count for course
            };
            model.ExistingModules.Add(module);
            course.Modules.Add(module);
            _context.Modules.Add(module);

            await _context.SaveChangesAsync();

            // Redirect to content creation page
            return RedirectToAction("CreateContent", new { moduleId = module.Id });
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
            return RedirectToAction("CreateContent", new { moduleId = module.Id });
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
    

    #endregion Teacher create section part

#region Teacher edit section part

        // Add these actions to your CourseController class within the #region Teacher edit section part

        // GET: Course/Edit/{id}
        public async Task<IActionResult> Edit(int id)
            {
                // Get the course with all related data
                var course = await _context.Courses
                    .Include(c => c.Teacher)
                    .Include(c => c.Modules.OrderBy(m => m.Order))
                        .ThenInclude(m => m.Contents.OrderBy(c => c.Order))
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (course == null)
                {
                    return NotFound();
                }

                // Check if the current teacher is the owner of the course
                var teacherId = HttpContext.Session.GetInt32("TeacherId");
                if (course.TeacherId != teacherId)
                {
                    return Unauthorized();
                }

                // Create view model
                var viewModel = new CourseEditViewModel
                {
                    CourseId = course.Id,
                    Title = course.Title,
                    Description = course.Description,
                    Category = course.Category,
                    ImageUrl = course.ImageUrl,
                    Modules = course.Modules.ToList()
                };

                return View(viewModel);
            }

            // POST: Course/UpdateCourse
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> UpdateCourse(CourseEditViewModel model)
            {

                var course = await _context.Courses.FindAsync(model.CourseId);
                if (course == null)
                {
                    return NotFound();
                }

                // Check if the current teacher is the owner of the course
                var teacherId = HttpContext.Session.GetInt32("TeacherId");
                if (course.TeacherId != teacherId)
                {
                    return Unauthorized();
                }

                // Update course properties
                course.Title = model.Title;
                course.Description = model.Description;
                course.Category = model.Category;

                // Handle image upload if a new image is provided
                if (model.ImageFile != null)
                {
                    string courseImagesFolder = Path.Combine(_hostEnvironment.WebRootPath, "img", "courses");
                    if (!Directory.Exists(courseImagesFolder))
                    {
                        Directory.CreateDirectory(courseImagesFolder);
                    }

                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(course.ImageUrl) && course.ImageUrl.StartsWith("~/img/courses/"))
                    {
                        string oldImagePath = Path.Combine(_hostEnvironment.WebRootPath,
                            course.ImageUrl.Replace("~/", "").Replace("/", Path.DirectorySeparatorChar.ToString()));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Save new image
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.ImageFile.FileName);
                    string filePath = Path.Combine(courseImagesFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(fileStream);
                    }

                    course.ImageUrl = "~/img/courses/" + uniqueFileName;
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Course updated successfully.";
                return RedirectToAction("Edit", new { id = course.Id });
            }

            // GET: Course/EditModule/{id}
            public async Task<IActionResult> EditModule(int id)
            {
                var module = await _context.Modules
                    .Include(m => m.Course)
                    .Include(m => m.Contents.OrderBy(c => c.Order))
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (module == null)
                {
                    return NotFound();
                }

                // Check if the current teacher is the owner of the course
                var teacherId = HttpContext.Session.GetInt32("TeacherId");
                if (module.Course.TeacherId != teacherId)
                {
                    return Unauthorized();
                }

                var viewModel = new ModuleEditViewModel
                {
                    ModuleId = module.Id,
                    Title = module.Title,
                    Description = module.Description,
                    CourseId = module.CourseId,
                    CourseTitle = module.Course.Title,
                    Contents = module.Contents.ToList()
                };

                return View(viewModel);
            }

            // POST: Course/UpdateModule
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> UpdateModule(ModuleEditViewModel model)
            {
                if (!ModelState.IsValid)
                {
                    return View("EditModule", model);
                }

                var module = await _context.Modules
                    .Include(m => m.Course)
                    .FirstOrDefaultAsync(m => m.Id == model.ModuleId);

                if (module == null)
                {
                    return NotFound();
                }

                // Check if the current teacher is the owner of the course
                var teacherId = HttpContext.Session.GetInt32("TeacherId");
                if (module.Course.TeacherId != teacherId)
                {
                    return Unauthorized();
                }

                // Update module properties
                module.Title = model.Title;
                module.Description = model.Description;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Module updated successfully.";
                return RedirectToAction("EditModule", new { id = module.Id });
            }

            // POST: Course/DeleteModule
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> DeleteModule(int moduleId, int courseId)
            {
                var module = await _context.Modules
                    .Include(m => m.Course)
                    .Include(m => m.Contents)
                    .FirstOrDefaultAsync(m => m.Id == moduleId);

                if (module == null)
                {
                    return NotFound();
                }

                // Check if the current teacher is the owner of the course
                var teacherId = HttpContext.Session.GetInt32("TeacherId");
                if (module.Course.TeacherId != teacherId)
                {
                    return Unauthorized();
                }

                // Delete related content files
                foreach (var content in module.Contents)
                {
                    if (content is VideoContent videoContent)
                    {
                        DeleteContentFile(videoContent.VideoLocation);
                    }
                    else if (content is TextContent textContent)
                    {
                        DeleteContentFile(textContent.TextLocation);
                    }
                    else if (content is QuizContent quizContent)
                    {
                        // Delete quiz questions
                        var questions = await _context.Set<QuizQuestion>()
                            .Where(q => q.QuizContentId == quizContent.Id)
                            .ToListAsync();
                        _context.QuizQuestions.RemoveRange(questions);
                    }

                    // Remove all related content progresses
                    var contentProgresses = await _context.Set<ContentProgress>()
                        .Where(cp => cp.ContentId == content.Id)
                        .ToListAsync();
                    _context.Set<ContentProgress>().RemoveRange(contentProgresses);
                }

                // Remove module from database
                _context.Modules.Remove(module);

                // Reorder remaining modules
                var remainingModules = await _context.Modules
                    .Where(m => m.CourseId == courseId && m.Id != moduleId)
                    .OrderBy(m => m.Order)
                    .ToListAsync();

                for (int i = 0; i < remainingModules.Count; i++)
                {
                    remainingModules[i].Order = i + 1;
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Module deleted successfully.";
                return RedirectToAction("Edit", new { id = courseId });
            }

            // POST: Course/DeleteContent
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> DeleteContent(int contentId, int moduleId)
            {
                var content = await _context.Set<ModuleContent>()
                    .Include(c => c.Module)
                        .ThenInclude(m => m.Course)
                    .FirstOrDefaultAsync(c => c.Id == contentId);

                if (content == null)
                {
                    return NotFound();
                }

                // Check if the current teacher is the owner of the course
                var teacherId = HttpContext.Session.GetInt32("TeacherId");
                if (content.Module.Course.TeacherId != teacherId)
                {
                    return Unauthorized();
                }

                // Delete content file if exists
                if (content is VideoContent videoContent)
                {
                    DeleteContentFile(videoContent.VideoLocation);
                }
                else if (content is TextContent textContent)
                {
                    DeleteContentFile(textContent.TextLocation);
                }
                else if (content is QuizContent quizContent)
                {
                    // Delete quiz questions
                    var questions = await _context.Set<QuizQuestion>()
                        .Where(q => q.QuizContentId == quizContent.Id)
                        .ToListAsync();
                    _context.QuizQuestions.RemoveRange(questions);
                }

                // Remove all related content progresses
                var contentProgresses = await _context.Set<ContentProgress>()
                    .Where(cp => cp.ContentId == content.Id)
                    .ToListAsync();
                _context.Set<ContentProgress>().RemoveRange(contentProgresses);

                // Remove content from database
                _context.Set<ModuleContent>().Remove(content);

                // Reorder remaining contents
                var remainingContents = await _context.Set<ModuleContent>()
                    .Where(c => c.ModuleId == moduleId && c.Id != contentId)
                    .OrderBy(c => c.Order)
                    .ToListAsync();

                for (int i = 0; i < remainingContents.Count; i++)
                {
                    remainingContents[i].Order = i + 1;
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Content deleted successfully.";
                return RedirectToAction("EditModule", new { id = moduleId });
            }

            // Helper method to delete content files
            private void DeleteContentFile(string filePath)
            {
                if (!string.IsNullOrEmpty(filePath) && filePath.StartsWith("~/"))
                {
                    string physicalPath = Path.Combine(_hostEnvironment.WebRootPath,
                        filePath.Replace("~/", "").Replace("/", Path.DirectorySeparatorChar.ToString()));
                    if (System.IO.File.Exists(physicalPath))
                    {
                        System.IO.File.Delete(physicalPath);
                    }
                }
            }

            // POST: Course/ReorderModules
            [HttpPost]
            public async Task<IActionResult> ReorderModules([FromBody] ReorderModulesViewModel model)
            {
                if (model == null || model.ModuleIds == null || !model.ModuleIds.Any())
                {
                    return BadRequest();
                }

                // Get first module to check course ownership
                var firstModule = await _context.Modules
                    .Include(m => m.Course)
                    .FirstOrDefaultAsync(m => m.Id == model.ModuleIds.First());

                if (firstModule == null)
                {
                    return NotFound();
                }

                // Check if the current teacher is the owner of the course
                var teacherId = HttpContext.Session.GetInt32("TeacherId");
                if (firstModule.Course.TeacherId != teacherId)
                {
                    return Unauthorized();
                }

                // Update order of modules
                for (int i = 0; i < model.ModuleIds.Count; i++)
                {
                    var module = await _context.Modules.FindAsync(model.ModuleIds[i]);
                    if (module != null)
                    {
                        module.Order = i + 1;
                    }
                }

                await _context.SaveChangesAsync();
                return Ok();
            }

            // POST: Course/ReorderContents
            [HttpPost]
            public async Task<IActionResult> ReorderContents([FromBody] ReorderContentsViewModel model)
            {
                if (model == null || model.ContentIds == null || !model.ContentIds.Any())
                {
                    return BadRequest();
                }

                // Get first content to check module and course ownership
                var firstContent = await _context.Set<ModuleContent>()
                    .Include(c => c.Module)
                        .ThenInclude(m => m.Course)
                    .FirstOrDefaultAsync(c => c.Id == model.ContentIds.First());

                if (firstContent == null)
                {
                    return NotFound();
                }

                // Check if the current teacher is the owner of the course
                var teacherId = HttpContext.Session.GetInt32("TeacherId");
                if (firstContent.Module.Course.TeacherId != teacherId)
                {
                    return Unauthorized();
                }

                // Update order of contents
                for (int i = 0; i < model.ContentIds.Count; i++)
                {
                    var content = await _context.Set<ModuleContent>().FindAsync(model.ContentIds[i]);
                    if (content != null)
                    {
                        content.Order = i + 1;
                    }
                }

                await _context.SaveChangesAsync();
                return Ok();
            }

            // POST: Course/EditQuiz/{id}
            public async Task<IActionResult> EditQuiz(int id)
            {
                var quizContent = await _context.Set<QuizContent>()
                    .Include(q => q.Module)
                        .ThenInclude(m => m.Course)
                    .Include(q => q.Questions)
                    .FirstOrDefaultAsync(q => q.Id == id);

                if (quizContent == null)
                {
                    return NotFound();
                }

                // Check if the current teacher is the owner of the course
                var teacherId = HttpContext.Session.GetInt32("TeacherId");
                if (quizContent.Module.Course.TeacherId != teacherId)
                {
                    return Unauthorized();
                }

                var viewModel = new QuizEditViewModel
                {
                    QuizContentId = quizContent.Id,
                    Title = quizContent.Title,
                    ShortDescription = quizContent.ShortDescription,
                    PassingScore = quizContent.PassingScore,
                    MaxAttempts = quizContent.MaxAttempts,
                    ModuleId = quizContent.ModuleId,
                    Questions = quizContent.Questions.ToList()
                };

                return View(viewModel);
            }

            // POST: Course/UpdateQuiz
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> UpdateQuiz(QuizEditViewModel model)
            {
                if (!ModelState.IsValid)
                {
                    return View("EditQuiz", model);
                }

                var quizContent = await _context.Set<QuizContent>()
                    .Include(q => q.Module)
                        .ThenInclude(m => m.Course)
                    .FirstOrDefaultAsync(q => q.Id == model.QuizContentId);

                if (quizContent == null)
                {
                    return NotFound();
                }

                // Check if the current teacher is the owner of the course
                var teacherId = HttpContext.Session.GetInt32("TeacherId");
                if (quizContent.Module.Course.TeacherId != teacherId)
                {
                    return Unauthorized();
                }

                // Update quiz properties
                quizContent.Title = model.Title;
                quizContent.ShortDescription = model.ShortDescription;
                quizContent.PassingScore = model.PassingScore;
                quizContent.MaxAttempts = model.MaxAttempts;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Quiz updated successfully.";
                return RedirectToAction("EditQuiz", new { id = quizContent.Id });
            }

            // POST: Course/DeleteQuestion
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> DeleteQuestion(int questionId, int quizContentId)
            {
                var question = await _context.Set<QuizQuestion>()
                    .Include(q => q.QuizContent)
                        .ThenInclude(qc => qc.Module)
                            .ThenInclude(m => m.Course)
                    .FirstOrDefaultAsync(q => q.Id == questionId);

                if (question == null)
                {
                    return NotFound();
                }

                // Check if the current teacher is the owner of the course
                var teacherId = HttpContext.Session.GetInt32("TeacherId");
                if (question.QuizContent.Module.Course.TeacherId != teacherId)
                {
                    return Unauthorized();
                }

                // Remove question from database
                _context.QuizQuestions.Remove(question);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Question deleted successfully.";
                return RedirectToAction("EditQuiz", new { id = quizContentId });
            }

            // POST: Course/AddQuizQuestion
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> AddQuizQuestion(QuizEditViewModel model)
            {
                if (string.IsNullOrEmpty(model.NewQuestion.Question) ||
                    string.IsNullOrEmpty(model.NewQuestion.Option1) ||
                    string.IsNullOrEmpty(model.NewQuestion.Option2))
                {
                    TempData["ErrorMessage"] = "Question and at least two options are required.";
                    return RedirectToAction("EditQuiz", new { id = model.QuizContentId });
                }

                var quizContent = await _context.Set<QuizContent>()
                    .Include(q => q.Module)
                        .ThenInclude(m => m.Course)
                    .FirstOrDefaultAsync(q => q.Id == model.QuizContentId);

                if (quizContent == null)
                {
                    return NotFound();
                }

                // Check if the current teacher is the owner of the course
                var teacherId = HttpContext.Session.GetInt32("TeacherId");
                if (quizContent.Module.Course.TeacherId != teacherId)
                {
                    return Unauthorized();
                }

                // Create new question
                var newQuestion = new QuizQuestion
                {
                    Question = model.NewQuestion.Question,
                    Options = new List<string> {
                model.NewQuestion.Option1,
                model.NewQuestion.Option2,
                model.NewQuestion.Option3,
                model.NewQuestion.Option4
            },
                    CorrectOptionIndex = model.NewQuestion.CorrectOptionIndex,
                    Points = model.NewQuestion.Points > 0 ? model.NewQuestion.Points : 1,
                    QuizContentId = quizContent.Id
                };

                _context.QuizQuestions.Add(newQuestion);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Question added successfully.";
                return RedirectToAction("EditQuiz", new { id = model.QuizContentId });
            }

            // POST: Course/EditQuestion
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> EditQuestion(QuizQuestionEditViewModel model)
            {
                var question = await _context.Set<QuizQuestion>()
                    .Include(q => q.QuizContent)
                        .ThenInclude(qc => qc.Module)
                            .ThenInclude(m => m.Course)
                    .FirstOrDefaultAsync(q => q.Id == model.QuestionId);

                if (question == null)
                {
                    return NotFound();
                }

                // Check if the current teacher is the owner of the course
                var teacherId = HttpContext.Session.GetInt32("TeacherId");
                if (question.QuizContent.Module.Course.TeacherId != teacherId)
                {
                    return Unauthorized();
                }

                // Update question
                question.Question = model.Question;
                question.Options = new List<string> { model.Option1, model.Option2, model.Option3, model.Option4 };
                question.CorrectOptionIndex = model.CorrectOptionIndex;
                question.Points = model.Points;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Question updated successfully.";
                return RedirectToAction("EditQuiz", new { id = question.QuizContentId });
            }

#endregion Teacher edit section part 


    }

}