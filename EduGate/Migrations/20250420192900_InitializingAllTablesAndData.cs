using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EduGate.Migrations
{
    /// <inheritdoc />
    public partial class InitializingAllTablesAndData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Teachers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teachers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Views = table.Column<int>(type: "int", nullable: false),
                    TeacherId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Courses_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CourseReviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Stars = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    CourseId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseReviews_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseReviews_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Modules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    CourseId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Modules_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentCourses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    EnrollmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentCourses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentCourses_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentCourses_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ModuleContents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ShortDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    ModuleId = table.Column<int>(type: "int", nullable: false),
                    PassingScore = table.Column<int>(type: "int", nullable: true),
                    MaxAttempts = table.Column<int>(type: "int", nullable: true),
                    TextLocation = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    VideoLocation = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DurationSeconds = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModuleContents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModuleContents_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContentProgresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    ContentId = table.Column<int>(type: "int", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Score = table.Column<int>(type: "int", nullable: true),
                    Attempts = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContentProgresses_ModuleContents_ContentId",
                        column: x => x.ContentId,
                        principalTable: "ModuleContents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContentProgresses_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuizQuestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Question = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Options = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CorrectOptionIndex = table.Column<int>(type: "int", nullable: false),
                    Points = table.Column<int>(type: "int", nullable: false),
                    QuizContentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizQuestions_ModuleContents_QuizContentId",
                        column: x => x.QuizContentId,
                        principalTable: "ModuleContents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Students",
                columns: new[] { "Id", "Email", "Name", "Password" },
                values: new object[,]
                {
                    { 1, "studentA@edugate.com", "Student A", "password789" },
                    { 2, "studentB@edugate.com", "Student B", "password012" }
                });

            migrationBuilder.InsertData(
                table: "Teachers",
                columns: new[] { "Id", "Email", "Name", "Password" },
                values: new object[,]
                {
                    { 1, "john.doe@edugate.com", "John Doe", "password123" },
                    { 2, "jane.smith@edugate.com", "Jane Smith", "password456" },
                    { 3, "ray.hansen@edugate.com", "Ray Hansen", "password456" }
                });

            migrationBuilder.InsertData(
                table: "Courses",
                columns: new[] { "Id", "Category", "Description", "ImageUrl", "TeacherId", "Title", "Views" },
                values: new object[,]
                {
                    { 1, "Programming", "Learn the fundamentals of C# programming, including syntax, OOP principles, and basic applications.", "~/img/csharp.jpg", 1, "Introduction to C#", 1500 },
                    { 2, "Programming", "Deep dive into Python's advanced features like decorators, generators, and data analysis libraries.", "~/img/python.png", 1, "Advanced Python", 1200 },
                    { 3, "Programming", "An entry-level course covering the basics of Java programming and object-oriented design.", "~/img/java.jpeg", 2, "Java for Beginners", 1100 },
                    { 4, "AI & Data Science", "Explore the basics of machine learning including supervised learning, classification, and regression.", "~/img/machine learning.jpg", 2, "Machine Learning Basics", 1800 },
                    { 5, "AI & Data Science", "Build deep learning models using TensorFlow, focusing on neural networks and CNNs.", "~/img/tensorflow.jpg", 3, "Deep Learning with TensorFlow", 950 },
                    { 6, "AI & Data Science", "Analyze, visualize, and manipulate data using Python's powerful Pandas library.", "~/img/pandas.png", 3, "Data Analysis with Pandas", 700 },
                    { 7, "Web Development", "Build modern web applications using ASP.NET Core MVC and Razor Pages.", "~/img/aspnetcore.jpg", 2, "Web Development with ASP.NET Core", 900 },
                    { 8, "Web Development", "Master React fundamentals, component-based architecture, hooks, and state management.", "~/img/react.jpg", 1, "Frontend Development with React", 1300 },
                    { 9, "Web Development", "Start your web development journey with the basics of HTML and CSS.", "~/img/htmlcss.jpg", 1, "HTML & CSS for Beginners", 800 }
                });

            migrationBuilder.InsertData(
                table: "CourseReviews",
                columns: new[] { "Id", "Comment", "CourseId", "CreatedDate", "Stars", "StudentId" },
                values: new object[,]
                {
                    { 1, "This course exceeded my expectations! The content was well-structured and the instructor explained complex concepts in a way that was easy to understand.", 1, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, 1 },
                    { 2, "Great course overall. Some sections could have more depth, but I learned a lot and would recommend it to others interested in this topic.", 1, new DateTime(2025, 3, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), 4, 2 },
                    { 3, "The instructor's approach to teaching Python's advanced concepts made them much easier to grasp. Highly recommend!", 2, new DateTime(2025, 2, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, 1 },
                    { 4, "Solid introduction to machine learning concepts, though I would have appreciated more practical examples.", 4, new DateTime(2025, 1, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, 2 }
                });

            migrationBuilder.InsertData(
                table: "Modules",
                columns: new[] { "Id", "CourseId", "Description", "Order", "Title" },
                values: new object[,]
                {
                    { 1, 3, "Learn the fundamentals of programming and how to begin your journey with Java.", 1, "Module 1: Getting Started" },
                    { 2, 2, "Discover the core concepts and principles of machine learning.", 1, "Module 1: ML Fundamentals" },
                    { 3, 1, "Understand how variables work in C#, their types, and how to use them effectively.", 1, "Module 1: Variables" },
                    { 4, 1, "Learn how to use conditional logic in C# using if, else if, and else statements.", 2, "Module 2: If condition" },
                    { 5, 1, "Explore how to use for loops to repeat actions in C# efficiently.", 3, "Module 3: For loop" }
                });

            migrationBuilder.InsertData(
                table: "ModuleContents",
                columns: new[] { "Id", "ContentType", "DurationSeconds", "ModuleId", "Order", "ShortDescription", "Title", "VideoLocation" },
                values: new object[,]
                {
                    { 1, "Video", null, 3, 2, "Watch this video to understand how variables work in C#.", "C# variables explanation", "~/contents/vid/IntroToCSharp/CSharpVariables.mp4" },
                    { 2, "Video", null, 4, 2, "This video explains how to use if, else if, and else statements in C#.", "C# if conditions explanation", "~/contents/vid/IntroToCSharp/CSharpIfStatement.mp4" },
                    { 3, "Video", null, 5, 2, "Understand how to use for loops in C# with practical examples.", "C# for loops explanation", "~/contents/vid/IntroToCSharp/CSharpForLoops.mp4" }
                });

            migrationBuilder.InsertData(
                table: "ModuleContents",
                columns: new[] { "Id", "ContentType", "ModuleId", "Order", "ShortDescription", "TextLocation", "Title" },
                values: new object[,]
                {
                    { 4, "Text", 3, 1, "Detailed text explanation on declaring and using variables in C#.", "~/contents/txt/IntroToCSharp/CSharpVariables.txt", "C# variables explanation" },
                    { 5, "Text", 4, 1, "Learn the syntax and logic behind if conditions in C#.", "~/contents/txt/IntroToCSharp/CSharpIfStatement.txt", "C# if conditions explanation" },
                    { 6, "Text", 5, 1, "Read how for loops function in C# and how to implement them.", "~/contents/txt/IntroToCSharp/CSharpForLoops.txt", "C# for loops explanation" }
                });

            migrationBuilder.InsertData(
                table: "ModuleContents",
                columns: new[] { "Id", "ContentType", "MaxAttempts", "ModuleId", "Order", "PassingScore", "ShortDescription", "Title" },
                values: new object[,]
                {
                    { 7, "Quiz", null, 1, 2, 70, "Test your understanding of general programming fundamentals.", "Programming Basics Quiz" },
                    { 8, "Quiz", null, 2, 2, 70, "Evaluate your knowledge of machine learning fundamentals.", "Machine Learning Concepts Quiz" },
                    { 9, "Quiz", null, 3, 3, 50, "Check your knowledge of variable types, naming, and usage in C#.", "C# Variables Quiz" },
                    { 10, "Quiz", null, 4, 3, 50, "Assess your understanding of conditional statements in C#.", "C# Conditions Quiz" },
                    { 11, "Quiz", null, 5, 3, 50, "Test how well you understand looping mechanisms like 'for' loops in C#.", "C# Loops Quiz" }
                });

            migrationBuilder.InsertData(
                table: "QuizQuestions",
                columns: new[] { "Id", "CorrectOptionIndex", "Options", "Points", "Question", "QuizContentId" },
                values: new object[,]
                {
                    { 1, 0, "A container for data;A type of loop;A function;None of the above", 1, "What is a variable?", 7 },
                    { 2, 1, "Model too simple;Model too complex;Model has low accuracy;Model has high bias", 1, "What is overfitting in machine learning?", 8 },
                    { 3, 2, "int = age 25;age int = 25;int age = 25;int: age = 25", 1, "Which of the following is a correct variable declaration in C#?", 9 },
                    { 4, 1, "int;bool;string;char", 1, "Which type is used to store a true or false value in C#?", 9 },
                    { 5, 3, "Declares a number;Declares a boolean;Declares a character;Declares a text variable", 1, "What does 'string name = \"Alice\";' do?", 9 },
                    { 6, 2, "for;switch;if;loop", 1, "Which keyword is used for checking a condition in C#?", 10 },
                    { 7, 1, "Adult;Minor;Error;Nothing", 1, "What will the following code print?\n\nint age = 16;\nif (age >= 18) {\n Console.WriteLine(\"Adult\");\n} else {\n Console.WriteLine(\"Minor\");\n}", 10 },
                    { 8, 1, "=;==;===;!=", 1, "Which operator is used for comparison in conditions?", 10 },
                    { 9, 0, "0 1 2;1 2 3;0 1 2 3;1 2", 1, "What is the output of this loop?\n\nfor (int i = 0; i < 3; i++) {\n Console.WriteLine(i);\n}", 11 },
                    { 10, 2, "Initialization;Condition;Update;Break", 1, "Which part of the 'for' loop runs after each iteration?", 11 },
                    { 11, 0, "for (;;);for (int i = 0; i < 1; i++);for (int i = 0; i < 10; i++);for (int i = 10; i > 0; i--)", 1, "Which of the following creates an infinite loop?", 11 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContentProgresses_ContentId",
                table: "ContentProgresses",
                column: "ContentId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentProgresses_StudentId",
                table: "ContentProgresses",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseReviews_CourseId",
                table: "CourseReviews",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseReviews_StudentId",
                table: "CourseReviews",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_TeacherId",
                table: "Courses",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_ModuleContents_ModuleId",
                table: "ModuleContents",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_Modules_CourseId",
                table: "Modules",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizQuestions_QuizContentId",
                table: "QuizQuestions",
                column: "QuizContentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentCourses_CourseId",
                table: "StudentCourses",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentCourses_StudentId",
                table: "StudentCourses",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContentProgresses");

            migrationBuilder.DropTable(
                name: "CourseReviews");

            migrationBuilder.DropTable(
                name: "QuizQuestions");

            migrationBuilder.DropTable(
                name: "StudentCourses");

            migrationBuilder.DropTable(
                name: "ModuleContents");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "Modules");

            migrationBuilder.DropTable(
                name: "Courses");

            migrationBuilder.DropTable(
                name: "Teachers");
        }
    }
}
