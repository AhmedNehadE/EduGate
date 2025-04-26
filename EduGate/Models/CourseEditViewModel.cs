using System.ComponentModel.DataAnnotations;

namespace EduGate.Models
{
    public class CourseEditViewModel
    {
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public string Category { get; set; }

        public string ImageUrl { get; set; }

        [Display(Name = "Course Image")]
        public IFormFile ImageFile { get; set; }

        public List<Module> Modules { get; set; } = new List<Module>();
    }

    public class ModuleEditViewModel
    {
        public int ModuleId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        public int CourseId { get; set; }

        public string CourseTitle { get; set; }

        public List<ModuleContent> Contents { get; set; } = new List<ModuleContent>();
    }

    public class QuizEditViewModel
    {
        public int QuizContentId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string ShortDescription { get; set; }

        [Range(1, 100, ErrorMessage = "Passing score must be between 1 and 100")]
        public int PassingScore { get; set; } = 70;

        public int? MaxAttempts { get; set; }

        public int ModuleId { get; set; }

        public List<QuizQuestion> Questions { get; set; } = new List<QuizQuestion>();

        // For adding a new question
        public QuizQuestionEditViewModel NewQuestion { get; set; } = new QuizQuestionEditViewModel();
    }

    public class QuizQuestionEditViewModel
    {
        public int QuestionId { get; set; }

        [Required(ErrorMessage = "Question is required")]
        public string Question { get; set; }

        [Required(ErrorMessage = "Option 1 is required")]
        public string Option1 { get; set; }

        [Required(ErrorMessage = "Option 2 is required")]
        public string Option2 { get; set; }

        public string Option3 { get; set; }

        public string Option4 { get; set; }

        [Range(0, 3, ErrorMessage = "Correct option index must be between 0 and 3")]
        public int CorrectOptionIndex { get; set; }

        [Range(1, 100, ErrorMessage = "Points must be between 1 and 100")]
        public int Points { get; set; } = 1;

        public int QuizContentId { get; set; }
    }

    public class ReorderModulesViewModel
    {
        public List<int> ModuleIds { get; set; }
    }

    public class ReorderContentsViewModel
    {
        public List<int> ContentIds { get; set; }
    }
}
