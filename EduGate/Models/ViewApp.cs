namespace EduGate.Models
{
    public class HomePageViewModel
    {
        public List<Course> MostViewedCourses { get; set; }
        public List<CourseCategory> CourseCategories { get; set; }
    }

    public class CourseCategory
    {
        public string Name { get; set; }
        public List<Course> Courses { get; set; }
    }

}
