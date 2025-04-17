using System;
using EduGate.Models;
using Microsoft.EntityFrameworkCore;

namespace EduGate.Data
{
    public class ApplicationDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<QuizContent> Quizzes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure entity inheritance - use Table-Per-Hierarchy
            modelBuilder.Entity<ModuleContent>().ToTable("ModuleContents");

            // Configure the discriminator
            modelBuilder.Entity<ModuleContent>()
                .HasDiscriminator(c => c.ContentType)
                .HasValue<TextContent>("Text")
                .HasValue<VideoContent>("Video")
                .HasValue<QuizContent>("Quiz");

            // Seed Teachers
            modelBuilder.Entity<Teacher>().HasData(
                new Teacher
                {
                    Id = 1,
                    Name = "John Doe",
                    Email = "john.doe@edugate.com",
                    Password = "password123"
                },
                new Teacher
                {
                    Id = 2,
                    Name = "Jane Smith",
                    Email = "jane.smith@edugate.com",
                    Password = "password456"
                },
                new Teacher
                {
                    Id = 3,
                    Name = "Ray Hansen",
                    Email = "ray.hansen@edugate.com",
                    Password = "password456"
                }
            );

            // Add seed data for Courses
            modelBuilder.Entity<Course>().HasData(
                // Programming Category
                new Course
                {
                    Id = 1,
                    Title = "Introduction to C#",
                    Category = "Programming",
                    ImageUrl = "img/csharp.jpg",
                    Views = 1500,
                    Description = "Learn the fundamentals of C# programming, including syntax, OOP principles, and basic applications.",
                    TeacherId = 1
                },
                new Course
                {
                    Id = 2,
                    Title = "Advanced Python",
                    Category = "Programming",
                    ImageUrl = "img/python.png",
                    Views = 1200,
                    Description = "Deep dive into Python's advanced features like decorators, generators, and data analysis libraries.",
                    TeacherId = 1
                },
                new Course
                {
                    Id = 3,
                    Title = "Java for Beginners",
                    Category = "Programming",
                    ImageUrl = "img/java.jpeg",
                    Views = 1100,
                    Description = "An entry-level course covering the basics of Java programming and object-oriented design.",
                    TeacherId = 2
                },

                // AI & Data Science Category
                new Course
                {
                    Id = 4,
                    Title = "Machine Learning Basics",
                    Category = "AI & Data Science",
                    ImageUrl = "img/machine learning.jpg",
                    Views = 1800,
                    Description = "Explore the basics of machine learning including supervised learning, classification, and regression.",
                    TeacherId = 2
                },
                new Course
                {
                    Id = 5,
                    Title = "Deep Learning with TensorFlow",
                    Category = "AI & Data Science",
                    ImageUrl = "img/tensorflow.jpg",
                    Views = 950,
                    Description = "Build deep learning models using TensorFlow, focusing on neural networks and CNNs.",
                    TeacherId = 3
                },
                new Course
                {
                    Id = 6,
                    Title = "Data Analysis with Pandas",
                    Category = "AI & Data Science",
                    ImageUrl = "img/pandas.png",
                    Views = 700,
                    Description = "Analyze, visualize, and manipulate data using Python's powerful Pandas library.",
                    TeacherId = 3
                },

                // Web Development Category
                new Course
                {
                    Id = 7,
                    Title = "Web Development with ASP.NET Core",
                    Category = "Web Development",
                    ImageUrl = "img/aspnetcore.jpg",
                    Views = 900,
                    Description = "Build modern web applications using ASP.NET Core MVC and Razor Pages.",
                    TeacherId = 2
                },
                new Course
                {
                    Id = 8,
                    Title = "Frontend Development with React",
                    Category = "Web Development",
                    ImageUrl = "img/react.jpg",
                    Views = 1300,
                    Description = "Master React fundamentals, component-based architecture, hooks, and state management.",
                    TeacherId = 1
                },
                new Course
                {
                    Id = 9,
                    Title = "HTML & CSS for Beginners",
                    Category = "Web Development",
                    ImageUrl = "img/htmlcss.jpg",
                    Views = 800,
                    Description = "Start your web development journey with the basics of HTML and CSS.",
                    TeacherId = 1
                }
            );

            // Seed Students
            modelBuilder.Entity<Student>().HasData(
                new Student
                {
                    Id = 1,
                    Name = "Student A",
                    Email = "studentA@edugate.com",
                    Password = "password789"
                },
                new Student
                {
                    Id = 2,
                    Name = "Student B",
                    Email = "studentB@edugate.com",
                    Password = "password012"
                }
            );

            // Seed Modules
            modelBuilder.Entity<Module>().HasData(
                new Module
                {
                    Id = 1,
                    Title = "Module 1: Getting Started",
                    Description = "Introduction to programming basics",
                    Order = 1,
                    CourseId = 1
                },
                new Module
                {
                    Id = 2,
                    Title = "Module 1: ML Fundamentals",
                    Description = "Machine learning core concepts",
                    Order = 1,
                    CourseId = 2
                }
            );

            // Seed QuizContent (instead of Quiz)
            modelBuilder.Entity<QuizContent>().HasData(
                new QuizContent
                {
                    Id = 1,
                    Title = "Programming Basics Quiz",
                    ShortDescription = "Test your knowledge of programming fundamentals",
                    ContentType = "Quiz",
                    Order = 2,
                    ModuleId = 1,
                    PassingScore = 70
                },
                new QuizContent
                {
                    Id = 2,
                    Title = "Machine Learning Concepts Quiz",
                    ShortDescription = "Test your understanding of ML concepts",
                    ContentType = "Quiz",
                    Order = 2,
                    ModuleId = 2,
                    PassingScore = 70
                }
            );

            // Seed QuizQuestions with updated foreign key
            modelBuilder.Entity<QuizQuestion>().HasData(
                new QuizQuestion
                {
                    Id = 1,
                    Question = "What is a variable?",
                    Options = new List<string> { "A container for data", "A type of loop", "A function", "None of the above" },
                    CorrectOptionIndex = 0,
                    QuizContentId = 1  // Link to QuizContent instead of Quiz
                },
                new QuizQuestion
                {
                    Id = 2,
                    Question = "What is overfitting in machine learning?",
                    Options = new List<string> { "Model too simple", "Model too complex", "Model has low accuracy", "Model has high bias" },
                    CorrectOptionIndex = 1,
                    QuizContentId = 2  // Link to QuizContent instead of Quiz
                }
            );

            // Course - Teacher (many-to-one)
            modelBuilder.Entity<Course>()
                .HasOne(c => c.Teacher)
                .WithMany(t => t.UploadedCourses)
                .HasForeignKey(c => c.TeacherId)
                .OnDelete(DeleteBehavior.Cascade);

            // Module - Course (many-to-one)
            modelBuilder.Entity<Module>()
                .HasOne(m => m.Course)
                .WithMany(c => c.Modules)
                .HasForeignKey(m => m.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // ModuleContent - Module (many-to-one)
            modelBuilder.Entity<ModuleContent>()
                .HasOne(mc => mc.Module)
                .WithMany(m => m.Contents)
                .HasForeignKey(mc => mc.ModuleId)
                .OnDelete(DeleteBehavior.Cascade);

            // QuizQuestion - QuizContent (many-to-one)
            modelBuilder.Entity<QuizQuestion>()
                .HasOne(q => q.QuizContent)
                .WithMany(qc => qc.Questions)
                .HasForeignKey(q => q.QuizContentId)
                .OnDelete(DeleteBehavior.Cascade);

            // StudentCourse mapping (instead of direct many-to-many)
            modelBuilder.Entity<StudentCourse>()
                .HasKey(sc => sc.Id);

            modelBuilder.Entity<StudentCourse>()
                .HasOne(sc => sc.Student)
                .WithMany(s => s.EnrolledCourses)
                .HasForeignKey(sc => sc.StudentId);

            modelBuilder.Entity<StudentCourse>()
                .HasOne(sc => sc.Course)
                .WithMany()
                .HasForeignKey(sc => sc.CourseId);

            // ContentProgress mapping
            modelBuilder.Entity<ContentProgress>()
                .HasKey(cp => cp.Id);

            modelBuilder.Entity<ContentProgress>()
                .HasOne(cp => cp.Student)
                .WithMany(s => s.ContentProgresses)
                .HasForeignKey(cp => cp.StudentId);

            modelBuilder.Entity<ContentProgress>()
                .HasOne(cp => cp.Content)
                .WithMany()
                .HasForeignKey(cp => cp.ContentId);

            // Course description length validation
            modelBuilder.Entity<Course>()
                .Property(c => c.Description)
                .HasMaxLength(1000)  // Updated to 1000 from 200 to allow more detailed descriptions
                .IsRequired();

            // QuizQuestion options (convert list to JSON or comma-separated string)
            modelBuilder.Entity<QuizQuestion>()
                .Property(q => q.Options)
                .HasConversion(
                    v => string.Join(";", v),
                    v => v.Split(";", StringSplitOptions.None).ToList()
                );
        }
    }
}