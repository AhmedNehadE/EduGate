namespace EduGate.Models
{
    public class CourseLearnViewModel
    {
        public Course Course { get; set; }

        // Dictionary mapping content ID to completion status
        public Dictionary<int, bool> StudentProgress { get; set; }

        // Dictionary mapping content ID to number of attempts
        public Dictionary<int, int> ContentAttempts { get; set; }

        // Dictionary mapping content ID to score
        public Dictionary<int, int> ContentScores { get; set; }

        // Helper method to get the current module
        public Module GetCurrentModule(int? selectedModuleId = null)
        {
            if (selectedModuleId.HasValue)
            {
                var selectedModule = Course.Modules.FirstOrDefault(m => m.Id == selectedModuleId.Value);
                if (selectedModule != null)
                    return selectedModule;
            }
            // If no module is selected or the selected module doesn't exist, find the first incomplete module
            foreach (var module in Course.Modules.OrderBy(m => m.Order))
            {
                bool allCompleted = module.Contents.All(c => StudentProgress.ContainsKey(c.Id) && StudentProgress[c.Id]);
                if (!allCompleted)
                    return module;
            }
            // If all modules are complete, return first module
            return Course.Modules.OrderBy(m => m.Order).FirstOrDefault();
        }

        // Helper method to get the next incomplete content in a module
        public ModuleContent GetNextIncompleteContent(Module module)
        {
            if (module == null) return null;
            foreach (var content in module.Contents.OrderBy(c => c.Order))
            {
                if (!StudentProgress.ContainsKey(content.Id) || !StudentProgress[content.Id])
                    return content;
            }
            // If all content in this module is complete, return the first content
            return module.Contents.OrderBy(c => c.Order).FirstOrDefault();
        }

        // Calculate progress percentage for the entire course
        public int GetOverallProgressPercentage()
        {
            int totalContents = Course.Modules.Sum(m => m.Contents.Count);
            if (totalContents == 0) return 0;
            int completedContents = StudentProgress.Count(kvp => kvp.Value);
            return (completedContents * 100) / totalContents;
        }

        // Calculate progress percentage for a specific module
        public int GetModuleProgressPercentage(Module module)
        {
            int totalContents = module.Contents.Count;
            if (totalContents == 0) return 0;
            int completedContents = module.Contents.Count(c =>
                StudentProgress.ContainsKey(c.Id) && StudentProgress[c.Id]);
            return (completedContents * 100) / totalContents;
        }
    }
}