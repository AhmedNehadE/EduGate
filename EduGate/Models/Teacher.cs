using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace EduGate.Models
{
    [Table("Teachers")]
    public class Teacher : IAccount
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

        // Navigation property for uploaded courses
        public List<Course> UploadedCourses { get; set; } = new List<Course>();

        public string GetAccountInfo()
        {
            return $"Teacher: {Name}, Email: {Email}, Uploaded Courses: {UploadedCourses.Count}";
        }


        public void AddQuizToModule(Module module, QuizContent quiz)
        {
            if (module != null && UploadedCourses.Any(c => c.Modules.Contains(module)))
            {
                module.Contents.Add(quiz);
            }
        }

        public void AddVideoToModule(Module module, VideoContent video)
        {
            if (module != null && UploadedCourses.Any(c => c.Modules.Contains(module)))
            {
                module.Contents.Add(video);
            }
        }

        public void AddTextContentToModule(Module module, TextContent text)
        {
            if (module != null && UploadedCourses.Any(c => c.Modules.Contains(module)))
            {
                module.Contents.Add(text);
            }
        }

    }
}