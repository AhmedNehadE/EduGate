using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EduGate.Models
{
    // Many-to-many relationship between students and courses
    [Table("StudentCourses")]
    public class StudentCourse
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Student")]
        public int StudentId { get; set; }
        public Student Student { get; set; }

        [ForeignKey("Course")]
        public int CourseId { get; set; }
        public Course Course { get; set; }

        // When the student enrolled
        public DateTime EnrollmentDate { get; set; } = DateTime.Now;

        // Whether the course has been completed
        public bool IsCompleted { get; set; }

        // When the course was completed
        public DateTime? CompletionDate { get; set; }
    }
}

