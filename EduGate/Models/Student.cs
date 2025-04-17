using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace EduGate.Models
{
    [Table("Students")]
    public class Student : IAccount
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 letters")]
        public string Password { get; set; }

        // Many-to-many relationship with courses
        public List<StudentCourse> EnrolledCourses { get; set; } = new List<StudentCourse>();

        // Track progress on individual content items
        public List<ContentProgress> ContentProgresses { get; set; } = new List<ContentProgress>();

        public string GetAccountInfo()
        {
            int completedCoursesCount = EnrolledCourses.Count(ec => ec.IsCompleted);
            return $"Student: {Name}, Email: {Email}, Enrolled: {EnrolledCourses.Count}, Completed: {completedCoursesCount}";
        }

        public void EnrollInCourse(Course course)
        {
            if (!EnrolledCourses.Any(ec => ec.CourseId == course.Id))
            {
                var enrollment = new StudentCourse
                {
                    StudentId = this.Id,
                    Student = this,
                    CourseId = course.Id,
                    Course = course,
                    EnrollmentDate = DateTime.Now,
                    IsCompleted = false
                };

                EnrolledCourses.Add(enrollment);
            }
        }

        public void CompleteCourse(Course course)
        {
            var enrollment = EnrolledCourses.FirstOrDefault(ec => ec.CourseId == course.Id);
            if (enrollment != null && !enrollment.IsCompleted)
            {
                enrollment.IsCompleted = true;
                enrollment.CompletionDate = DateTime.Now;
            }
        }

        public void CompleteContent(ModuleContent content)
        {
            var progress = ContentProgresses.FirstOrDefault(cp => cp.ContentId == content.Id);

            if (progress == null)
            {
                progress = new ContentProgress
                {
                    StudentId = this.Id,
                    Student = this,
                    ContentId = content.Id,
                    Content = content,
                    IsCompleted = true,
                    CompletedDate = DateTime.Now
                };

                ContentProgresses.Add(progress);
            }
            else if (!progress.IsCompleted)
            {
                progress.IsCompleted = true;
                progress.CompletedDate = DateTime.Now;
            }

            // Check if all content in the course is completed
            var moduleInCourse = content.Module;
            var course = moduleInCourse.Course;

            bool allContentCompleted = true;
            foreach (var module in course.Modules)
            {
                foreach (var moduleContent in module.Contents)
                {
                    if (!ContentProgresses.Any(cp => cp.ContentId == moduleContent.Id && cp.IsCompleted))
                    {
                        allContentCompleted = false;
                        break;
                    }
                }

                if (!allContentCompleted)
                    break;
            }

            if (allContentCompleted)
                CompleteCourse(course);
        }

        public int TakeQuiz(QuizContent quiz, List<int> answers)
        {
            // Find or create progress entry
            var progress = ContentProgresses.FirstOrDefault(cp => cp.ContentId == quiz.Id);

            if (progress == null)
            {
                progress = new ContentProgress
                {
                    StudentId = this.Id,
                    Student = this,
                    ContentId = quiz.Id,
                    Content = quiz,
                    Attempts = 0
                };

                ContentProgresses.Add(progress);
            }

            // Increment attempts
            progress.Attempts++;

            // Calculate score
            int totalPoints = quiz.Questions.Sum(q => q.Points);
            int earnedPoints = 0;

            for (int i = 0; i < answers.Count && i < quiz.Questions.Count; i++)
            {
                if (answers[i] == quiz.Questions[i].CorrectOptionIndex)
                {
                    earnedPoints += quiz.Questions[i].Points;
                }
            }

            int scorePercentage = totalPoints > 0 ? (earnedPoints * 100) / totalPoints : 0;
            progress.Score = scorePercentage;

            // Mark as completed if passed
            if (scorePercentage >= quiz.PassingScore)
            {
                progress.IsCompleted = true;
                progress.CompletedDate = DateTime.Now;
            }

            return scorePercentage;
        }

        public int GetCourseProgressPercentage(Course course)
        {
            int totalContents = course.Modules.SelectMany(m => m.Contents).Count();
            if (totalContents == 0) return 0;

            int completedContents = ContentProgresses
                .Count(p => course.Modules.SelectMany(m => m.Contents)
                    .Any(c => c.Id == p.ContentId) && p.IsCompleted);

            return (completedContents * 100) / totalContents;
        }
    }
}