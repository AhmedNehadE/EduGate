using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduGate.Models
{
    [Table("CourseReviews")]
    public class CourseReview
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars")]
        public int Stars { get; set; }

        [Required]
        [StringLength(1000, ErrorMessage = "Review comment can't exceed 1000 characters")]
        public string Comment { get; set; }

        // Date when the review was created
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Foreign key to Student
        [ForeignKey("Student")]
        public int StudentId { get; set; }
        public Student Student { get; set; }

        // Foreign key to Course
        [ForeignKey("Course")]
        public int CourseId { get; set; }
        public Course Course { get; set; }
    }

    [Table("Courses")]
    public class Course
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        [StringLength(200)]
        public string Category { get; set; }

        [Required]
        [StringLength(200)]
        public string ImageUrl { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; }

        [Range(0, int.MaxValue)]
        public int Views { get; set; }

        // Foreign key to Teacher
        [ForeignKey("Teacher")]
        public int TeacherId { get; set; }
        public Teacher Teacher { get; set; }

        // Course now has modules instead of direct videos/quizzes
        public List<Module> Modules { get; set; } = new List<Module>();

        // Add reviews collection
        public List<CourseReview> Reviews { get; set; } = new List<CourseReview>();

        // Helper method to calculate average rating
        [NotMapped]
        public double AverageRating => Reviews.Any() ? Reviews.Average(r => r.Stars) : 0;
    }

    [Table("Modules")]
    public class Module
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        // Order of this module in the course
        public int Order { get; set; }

        // Foreign key to Course
        [ForeignKey("Course")]
        public int CourseId { get; set; }
        public Course Course { get; set; }

        // Module contains different types of content
        public List<ModuleContent> Contents { get; set; } = new List<ModuleContent>();
    }

    // Abstract base class for all module content types
    public abstract class ModuleContent
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [StringLength(500)]
        public string ShortDescription { get; set; }

        // Order of this content in the module
        public int Order { get; set; }

        // Content type discriminator
        [Required]
        public string ContentType { get; set; }

        // Foreign key to Module
        [ForeignKey("Module")]
        public int ModuleId { get; set; }
        public Module Module { get; set; }
    }


    public class TextContent : ModuleContent
    {
        [Required]
        [StringLength(500)]
        public string TextLocation { get; set; }

        public TextContent()
        {
            ContentType = "Text";
        }
    }

    public class VideoContent : ModuleContent
    {
        [Required]
        [StringLength(500)]
        public string VideoLocation { get; set; }

        // Optional duration in seconds
        public int? DurationSeconds { get; set; }

        public VideoContent()
        {
            ContentType = "Video";
        }
    }

    public class QuizContent : ModuleContent
    {
        // Passing score (percentage)
        [Range(0, 100)]
        public int PassingScore { get; set; } = 70;

        // Maximum attempts allowed (null = unlimited)
        public int? MaxAttempts { get; set; }

        // Navigation to list of questions
        public List<QuizQuestion> Questions { get; set; } = new List<QuizQuestion>();

        public QuizContent()
        {
            ContentType = "Quiz";
        }
    }

    [Table("QuizQuestions")]
    public class QuizQuestion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(300, ErrorMessage = "Question must be at most 300 letters")]
        public string Question { get; set; }

        [Required]
        public List<string> Options { get; set; } = new List<string>();

        [Required]
        [Range(0, 3, ErrorMessage = "Correct option index must be between 0 and 3.")]
        public int CorrectOptionIndex { get; set; }

        // Points for this question
        [Range(1, 100)]
        public int Points { get; set; } = 1;

        // Foreign Key to QuizContent
        [ForeignKey("QuizContent")]
        public int QuizContentId { get; set; }
        public QuizContent QuizContent { get; set; }
    }

    // Student progress tracking classes
    [Table("ContentProgresses")]
    public class ContentProgress
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Student")]
        public int StudentId { get; set; }
        public Student Student { get; set; }

        [ForeignKey("ModuleContent")]
        public int ContentId { get; set; }
        public ModuleContent Content { get; set; }

        // Whether the content has been completed
        public bool IsCompleted { get; set; }

        // When the content was completed
        public DateTime? CompletedDate { get; set; }

        // For quiz contents, stores the latest score
        public int? Score { get; set; }

        // For quiz contents, stores the number of attempts
        public int Attempts { get; set; } = 0;
    }


}