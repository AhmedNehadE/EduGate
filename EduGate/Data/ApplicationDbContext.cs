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
        public DbSet<CourseReview> CourseReviews { get; set; }
        public DbSet<TextContent> TextContents { get; set; }
        public DbSet<VideoContent> VideoContents { get; set; }
        public DbSet<ModuleContent> ModuleContents { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<ContentProgress> ContentProgresses { get; set; }
        public DbSet<QuizQuestion> QuizQuestions { get; set; }
        public DbSet<StudentCourse> StudentCourses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ModuleContent>().ToTable("ModuleContents");


            modelBuilder.Entity<ModuleContent>()
                .HasDiscriminator(c => c.ContentType)
                .HasValue<TextContent>("Text")
                .HasValue<VideoContent>("Video")
                .HasValue<QuizContent>("Quiz");


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


            modelBuilder.Entity<Course>().HasData(

                new Course
                {
                    Id = 1,
                    Title = "Introduction to C#",
                    Category = "Programming",
                    ImageUrl = "~/img/csharp.jpg",
                    Views = 1500,
                    Description = "Learn the fundamentals of C# programming, including syntax, OOP principles, and basic applications.",
                    TeacherId = 1
                },
                new Course
                {
                    Id = 2,
                    Title = "Advanced Python",
                    Category = "Programming",
                    ImageUrl = "~/img/python.png",
                    Views = 1200,
                    Description = "Deep dive into Python's advanced features like decorators, generators, and data analysis libraries.",
                    TeacherId = 1
                },
                new Course
                {
                    Id = 3,
                    Title = "Java for Beginners",
                    Category = "Programming",
                    ImageUrl = "~/img/java.jpeg",
                    Views = 1100,
                    Description = "An entry-level course covering the basics of Java programming and object-oriented design.",
                    TeacherId = 2
                },


                new Course
                {
                    Id = 4,
                    Title = "Machine Learning Basics",
                    Category = "Data Science",
                    ImageUrl = "~/img/machine learning.jpg",
                    Views = 1800,
                    Description = "Explore the basics of machine learning including supervised learning, classification, and regression.",
                    TeacherId = 2
                },
                new Course
                {
                    Id = 5,
                    Title = "Deep Learning with TensorFlow",
                    Category = "Data Science",
                    ImageUrl = "~/img/tensorflow.jpg",
                    Views = 950,
                    Description = "Build deep learning models using TensorFlow, focusing on neural networks and CNNs.",
                    TeacherId = 3
                },
                new Course
                {
                    Id = 6,
                    Title = "Data Analysis with Pandas",
                    Category = "Data Science",
                    ImageUrl = "~/img/pandas.png",
                    Views = 700,
                    Description = "Analyze, visualize, and manipulate data using Python's powerful Pandas library.",
                    TeacherId = 3
                },


                new Course
                {
                    Id = 7,
                    Title = "Web Development with ASP.NET Core",
                    Category = "Web Development",
                    ImageUrl = "~/img/aspnetcore.jpg",
                    Views = 900,
                    Description = "Build modern web applications using ASP.NET Core MVC and Razor Pages.",
                    TeacherId = 2
                },
                new Course
                {
                    Id = 8,
                    Title = "Frontend Development with React",
                    Category = "Web Development",
                    ImageUrl = "~/img/react.jpg",
                    Views = 1300,
                    Description = "Master React fundamentals, component-based architecture, hooks, and state management.",
                    TeacherId = 1
                },
                new Course
                {
                    Id = 9,
                    Title = "HTML & CSS for Beginners",
                    Category = "Web Development",
                    ImageUrl = "~/img/htmlcss.jpg",
                    Views = 800,
                    Description = "Start your web development journey with the basics of HTML and CSS.",
                    TeacherId = 1
                }
            );


            modelBuilder.Entity<CourseReview>().HasData(
                new CourseReview
                {
                    Id = 1,
                    Stars = 5,
                    Comment = "This course exceeded my expectations! The content was well-structured and the instructor explained complex concepts in a way that was easy to understand.",
                    CreatedDate = new DateTime(2025, 1, 15),
                    StudentId = 1,
                    CourseId = 1
                },
                new CourseReview
                {
                    Id = 2,
                    Stars = 4,
                    Comment = "Great course overall. Some sections could have more depth, but I learned a lot and would recommend it to others interested in this topic.",
                    CreatedDate = new DateTime(2025, 3, 5),
                    StudentId = 2,
                    CourseId = 1
                },
                new CourseReview
                {
                    Id = 3,
                    Stars = 5,
                    Comment = "The instructor's approach to teaching Python's advanced concepts made them much easier to grasp. Highly recommend!",
                    CreatedDate = new DateTime(2025, 2, 20),
                    StudentId = 1,
                    CourseId = 2
                },
                new CourseReview
                {
                    Id = 4,
                    Stars = 3,
                    Comment = "Solid introduction to machine learning concepts, though I would have appreciated more practical examples.",
                    CreatedDate = new DateTime(2025, 1, 25),
                    StudentId = 2,
                    CourseId = 4
                }
            );

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

            modelBuilder.Entity<Module>().HasData(
                new Module
                {
                    Id = 1,
                    Title = "Module 1: Getting Started",
                    Description = "Learn the fundamentals of programming and how to begin your journey with Java.",
                    Order = 1,
                    CourseId = 3
                },
                new Module
                {
                    Id = 2,
                    Title = "Module 1: ML Fundamentals",
                    Description = "Discover the core concepts and principles of machine learning.",
                    Order = 1,
                    CourseId = 2
                },
                new Module
                {
                    Id = 3,
                    Title = "Module 1: Variables",
                    Description = "Understand how variables work in C#, their types, and how to use them effectively.",
                    Order = 1,
                    CourseId = 1
                },
                new Module
                {
                    Id = 4,
                    Title = "Module 2: If condition",
                    Description = "Learn how to use conditional logic in C# using if, else if, and else statements.",
                    Order = 2,
                    CourseId = 1
                },
                new Module
                {
                    Id = 5,
                    Title = "Module 3: For loop",
                    Description = "Explore how to use for loops to repeat actions in C# efficiently.",
                    Order = 3,
                    CourseId = 1
                }
            );

            modelBuilder.Entity<VideoContent>().HasData(
                new VideoContent
                {
                    Id = 1,
                    Title = "C# variables explanation",
                    ShortDescription = "Watch this video to understand how variables work in C#.",
                    ContentType = "Video",
                    Order = 2,
                    ModuleId = 3,
                    VideoLocation = "~/contents/vid/IntroToCSharp/CSharpVariables.mp4"
                },
                new VideoContent
                {
                    Id = 2,
                    Title = "C# if conditions explanation",
                    ShortDescription = "This video explains how to use if, else if, and else statements in C#.",
                    ContentType = "Video",
                    Order = 2,
                    ModuleId = 4,
                    VideoLocation = "~/contents/vid/IntroToCSharp/CSharpIfStatement.mp4"
                },
                new VideoContent
                {
                    Id = 3,
                    Title = "C# for loops explanation",
                    ShortDescription = "Understand how to use for loops in C# with practical examples.",
                    ContentType = "Video",
                    Order = 2,
                    ModuleId = 5,
                    VideoLocation = "~/contents/vid/IntroToCSharp/CSharpForLoops.mp4"
                }
            );

            modelBuilder.Entity<TextContent>().HasData(
                new TextContent
                {
                    Id = 4,
                    Title = "C# variables explanation",
                    ShortDescription = "Detailed text explanation on declaring and using variables in C#.",
                    ContentType = "Text",
                    Order = 1,
                    ModuleId = 3,
                    TextLocation = "~/contents/txt/IntroToCSharp/CSharpVariables.txt"
                },
                new TextContent
                {
                    Id = 5,
                    Title = "C# if conditions explanation",
                    ShortDescription = "Learn the syntax and logic behind if conditions in C#.",
                    ContentType = "Text",
                    Order = 1,
                    ModuleId = 4,
                    TextLocation = "~/contents/txt/IntroToCSharp/CSharpIfStatement.txt"
                },
                new TextContent
                {
                    Id = 6,
                    Title = "C# for loops explanation",
                    ShortDescription = "Read how for loops function in C# and how to implement them.",
                    ContentType = "Text",
                    Order = 1,
                    ModuleId = 5,
                    TextLocation = "~/contents/txt/IntroToCSharp/CSharpForLoops.txt"
                }
            );


            modelBuilder.Entity<QuizContent>().HasData(
                new QuizContent
                {
                    Id = 7,
                    Title = "Programming Basics Quiz",
                    ShortDescription = "Test your understanding of general programming fundamentals.",
                    ContentType = "Quiz",
                    Order = 2,
                    ModuleId = 1,
                    PassingScore = 70
                },
                new QuizContent
                {
                    Id = 8,
                    Title = "Machine Learning Concepts Quiz",
                    ShortDescription = "Evaluate your knowledge of machine learning fundamentals.",
                    ContentType = "Quiz",
                    Order = 2,
                    ModuleId = 2,
                    PassingScore = 70
                },
                new QuizContent
                {
                    Id = 9,
                    Title = "C# Variables Quiz",
                    ShortDescription = "Check your knowledge of variable types, naming, and usage in C#.",
                    ContentType = "Quiz",
                    Order = 3,
                    ModuleId = 3,
                    PassingScore = 50
                },
                new QuizContent
                {
                    Id = 10,
                    Title = "C# Conditions Quiz",
                    ShortDescription = "Assess your understanding of conditional statements in C#.",
                    ContentType = "Quiz",
                    Order = 3,
                    ModuleId = 4,
                    PassingScore = 50
                },
                new QuizContent
                {
                    Id = 11,
                    Title = "C# Loops Quiz",
                    ShortDescription = "Test how well you understand looping mechanisms like 'for' loops in C#.",
                    ContentType = "Quiz",
                    Order = 3,
                    ModuleId = 5,
                    PassingScore = 50
                }
            );



            modelBuilder.Entity<QuizQuestion>().HasData(
                new QuizQuestion
                {
                    Id = 1,
                    Question = "What is a variable?",
                    Options = new List<string> { "A container for data", "A type of loop", "A function", "None of the above" },
                    CorrectOptionIndex = 0,
                    QuizContentId = 7 
                },
                new QuizQuestion
                {
                    Id = 2,
                    Question = "What is overfitting in machine learning?",
                    Options = new List<string> { "Model too simple", "Model too complex", "Model has low accuracy", "Model has high bias" },
                    CorrectOptionIndex = 1,
                    QuizContentId = 8  
                },
                new QuizQuestion
                {
                    Id = 3,
                    Question = "Which of the following is a correct variable declaration in C#?",
                    Options = new List<string> { "int = age 25", "age int = 25", "int age = 25", "int: age = 25" },
                    CorrectOptionIndex = 2,
                    QuizContentId = 9
                },
                new QuizQuestion
                {
                    Id = 4,
                    Question = "Which type is used to store a true or false value in C#?",
                    Options = new List<string> { "int", "bool", "string", "char" },
                    CorrectOptionIndex = 1,
                    QuizContentId = 9
                },
                new QuizQuestion
                {
                    Id = 5,
                    Question = "What does 'string name = \"Alice\";' do?",
                    Options = new List<string> { "Declares a number", "Declares a boolean", "Declares a character", "Declares a text variable" },
                    CorrectOptionIndex = 3,
                    QuizContentId = 9
                },
                new QuizQuestion
                {
                    Id = 6,
                    Question = "Which keyword is used for checking a condition in C#?",
                    Options = new List<string> { "for", "switch", "if", "loop" },
                    CorrectOptionIndex = 2,
                    QuizContentId = 10
                },
                new QuizQuestion
                {
                    Id = 7,
                    Question = "What will the following code print?\n\nint age = 16;\nif (age >= 18) {\n Console.WriteLine(\"Adult\");\n} else {\n Console.WriteLine(\"Minor\");\n}",
                    Options = new List<string> { "Adult", "Minor", "Error", "Nothing" },
                    CorrectOptionIndex = 1,
                    QuizContentId = 10
                },
                new QuizQuestion
                {
                    Id = 8,
                    Question = "Which operator is used for comparison in conditions?",
                    Options = new List<string> { "=", "==", "===", "!=" },
                    CorrectOptionIndex = 1,
                    QuizContentId = 10
                },
                new QuizQuestion
                {
                    Id = 9,
                    Question = "What is the output of this loop?\n\nfor (int i = 0; i < 3; i++) {\n Console.WriteLine(i);\n}",
                    Options = new List<string> { "0 1 2", "1 2 3", "0 1 2 3", "1 2" },
                    CorrectOptionIndex = 0,
                    QuizContentId = 11
                },
                new QuizQuestion
                {
                    Id = 10,
                    Question = "Which part of the 'for' loop runs after each iteration?",
                    Options = new List<string> { "Initialization", "Condition", "Update", "Break" },
                    CorrectOptionIndex = 2,
                    QuizContentId = 11
                },
                new QuizQuestion
                {
                    Id = 11,
                    Question = "Which of the following creates an infinite loop?",
                    Options = new List<string> { "for (;;)", "for (int i = 0; i < 1; i++)", "for (int i = 0; i < 10; i++)", "for (int i = 10; i > 0; i--)" },
                    CorrectOptionIndex = 0,
                    QuizContentId = 11
                }
            );

            modelBuilder.Entity<Course>()
                .HasOne(c => c.Teacher)
                .WithMany(t => t.UploadedCourses)
                .HasForeignKey(c => c.TeacherId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Module>()
                .HasOne(m => m.Course)
                .WithMany(c => c.Modules)
                .HasForeignKey(m => m.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ModuleContent>()
                .HasOne(mc => mc.Module)
                .WithMany(m => m.Contents)
                .HasForeignKey(mc => mc.ModuleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QuizQuestion>()
                .HasOne(q => q.QuizContent)
                .WithMany(qc => qc.Questions)
                .HasForeignKey(q => q.QuizContentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CourseReview>()
                .HasOne(cr => cr.Course)
                .WithMany(c => c.Reviews)
                .HasForeignKey(cr => cr.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CourseReview>()
                .HasOne(cr => cr.Student)
                .WithMany()
                .HasForeignKey(cr => cr.StudentId)
                .OnDelete(DeleteBehavior.Restrict); 


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

            modelBuilder.Entity<Course>()
                .Property(c => c.Description)
                .HasMaxLength(1000)  
                .IsRequired();

            modelBuilder.Entity<QuizQuestion>()
                .Property(q => q.Options)
                .HasConversion(
                    v => string.Join(";", v),
                    v => v.Split(";", StringSplitOptions.None).ToList()
                );
        }
    }
}