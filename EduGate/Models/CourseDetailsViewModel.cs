namespace EduGate.Models
{
    public class CourseDetailsViewModel
    {
        public Course Course { get; set; }
        public List<Course> RelatedCourses { get; set; }
        public bool IsEnrolled { get; set; }
        public bool HasReviewed { get; set; }

        // For the review form
        public CourseReview NewReview { get; set; }
    }

}
