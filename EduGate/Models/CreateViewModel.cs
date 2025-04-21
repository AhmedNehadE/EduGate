
using EduGate.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EduGate.Models
{
    public class CourseCreateViewModel
    {
        [Required(ErrorMessage = "Course title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [StringLength(200, ErrorMessage = "Category cannot exceed 200 characters")]
        public string Category { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Course image is required")]
        [Display(Name = "Course Image")]
        public IFormFile ImageFile { get; set; }
    }

    public class ModuleCreateViewModel
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; }

        [Required(ErrorMessage = "Module title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; }

        public List<Module> ExistingModules { get; set; } = new List<Module>();
    }

    public class ContentCreateViewModel
    {
        public int ModuleId { get; set; }
        public string ModuleTitle { get; set; }
        public int CourseId { get; set; }
        public string CourseTitle { get; set; }

        [Required(ErrorMessage = "Content title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string ShortDescription { get; set; }

        [Required(ErrorMessage = "Content type is required")]
        [Display(Name = "Content Type")]
        public string ContentType { get; set; }

        // Video content properties
        [Display(Name = "Video File")]
        public IFormFile VideoFile { get; set; }

        [Display(Name = "Duration (seconds)")]
        [Range(0, int.MaxValue, ErrorMessage = "Duration must be a positive number")]
        public int? DurationSeconds { get; set; }

        // Text content properties
        [Display(Name = "Text File")]
        public IFormFile TextFile { get; set; }

        // Quiz content properties
        [Display(Name = "Passing Score (%)")]
        [Range(0, 100, ErrorMessage = "Passing score must be between 0 and 100")]
        public int? PassingScore { get; set; }

        [Display(Name = "Maximum Attempts (leave empty for unlimited)")]
        [Range(1, int.MaxValue, ErrorMessage = "Maximum attempts must be a positive number")]
        public int? MaxAttempts { get; set; }

        public List<ModuleContent> ExistingContents { get; set; } = new List<ModuleContent>();
    }

    public class QuizQuestionsViewModel
    {
        public int QuizContentId { get; set; }
        public string QuizTitle { get; set; }
        public int ModuleId { get; set; }

        [Required(ErrorMessage = "Question text is required")]
        [StringLength(300, ErrorMessage = "Question cannot exceed 300 characters")]
        public string Question { get; set; }

        [Required(ErrorMessage = "Option 1 is required")]
        [Display(Name = "Option 1")]
        public string Option1 { get; set; }

        [Required(ErrorMessage = "Option 2 is required")]
        [Display(Name = "Option 2")]
        public string Option2 { get; set; }

        [Required(ErrorMessage = "Option 3 is required")]
        [Display(Name = "Option 3")]
        public string Option3 { get; set; }

        [Required(ErrorMessage = "Option 4 is required")]
        [Display(Name = "Option 4")]
        public string Option4 { get; set; }

        [Required(ErrorMessage = "Correct option is required")]
        [Range(0, 3, ErrorMessage = "Correct option index must be between 0 and 3")]
        [Display(Name = "Correct Option (0-3)")]
        public int CorrectOptionIndex { get; set; }

        [Range(1, 100, ErrorMessage = "Points must be between 1 and 100")]
        [Display(Name = "Points")]
        public int Points { get; set; } = 1;

        public List<QuizQuestion> ExistingQuestions { get; set; } = new List<QuizQuestion>();
    }
}