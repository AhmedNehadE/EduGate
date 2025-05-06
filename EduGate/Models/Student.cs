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

    }
}