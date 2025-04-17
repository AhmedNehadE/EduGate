namespace EduGate.Models
{
    // View models for search functionality
    public class CourseSearchViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string TeacherName { get; set; }
        public string Category { get; set; }
        public string ImageUrl { get; set; }
        public int Views { get; set; }
        public int RelevanceScore { get; set; }
    }

    public class CourseSearchResultsViewModel
    {
        public string SearchQuery { get; set; }
        public List<CourseSearchViewModel> Courses { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalResults { get; set; }
    }
}
