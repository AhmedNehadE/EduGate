EduGate – Online Learning Platform
EduGate is an online learning web application developed using ASP.NET MVC. The platform allows users to either participate as students or contribute as teachers, offering a dynamic and interactive e-learning environment.

The system is built using Entity Framework for data access and managed through Microsoft SQL Server. Development tools include Visual Studio and SQL Server Management Studio (SSMS).

Features
Student Section
Register and enroll in any available course

Access course modules that include:

Text explanations

Video lessons

Quizzes

Submit one review per course and update it later

Track your learning progress (enrolled and completed courses)

View and manage a list of all enrolled courses

Un-enroll from any course at any time

Update your email and password from the profile page

Teacher Section
Register as a teacher and create new courses

Add modules to courses and include different types of content:

Text content

Video content

Quizzes with custom questions and passing thresholds

Edit, update, or delete any content or module

View all available courses

Access a personalized “My Courses” section that shows all created courses and their total view counts

Project Structure
Controllers
HomeController.cs

AccountController.cs

CourseController.cs

Views
Organized by controller folders:

Account Views:

Dashboard.cshtml

Index.cshtml

Login.cshtml

Profile.cshtml

Register.cshtml

RegisterTeacher.cshtml

Course Views:

Create.cshtml

CreateModule.cshtml

CreateContent.cshtml

CreateQuizQuestions.cshtml

Details.cshtml

Edit.cshtml

EditModule.cshtml

Index.cshtml

Learn.cshtml

Search.cshtml

Home Views:

Index.cshtml

Privacy.cshtml

Shared Views:

_Layout.cshtml

_Layout.cshtml.css

Error.cshtml

_ValidationScriptsPartial.cshtml

Models
Student.cs

Teacher.cs

Course.cs

Module.cs

ModuleContent.cs (base class for content types)

TextContent.cs, VideoContent.cs, QuizContent.cs

QuizQuestion.cs

StudentCourse.cs

ContentProgress.cs

CourseReview.cs

IAccount.cs

ViewApp.cs

ViewModels
CourseDetailsViewModel.cs

CourseLearnViewModel.cs

CourseSearchResultsViewModel.cs

CourseEditViewModel.cs

CreateViewModel.cs

ErrorViewModel.cs

Database Schema Overview
Core Entities
Student: Stores student info (ID, name, email, password)

Teacher: Stores teacher info (ID, name, email, password)

Course: Includes title, category, description (max 1000 chars), image URL, view count, and teacher reference

Module: Represents course chapters, ordered and linked to courses

ModuleContent: Base class for all content types, uses Table-Per-Hierarchy (TPH) inheritance

Content Types:

TextContent: File-based text materials

VideoContent: Linked video lectures

QuizContent: Includes questions and a passing score

Quiz Structure
QuizQuestion: Contains question text, multiple options (semicolon-separated), correct answer index, and linked to a quiz

Relationship Entities
StudentCourse: Junction table for enrollments (student ID + course ID)

ContentProgress: Tracks how far a student has progressed within course content

CourseReview: Stores student ratings and written feedback

Key Relationships
A teacher can create many courses (cascade delete)

A course contains multiple modules and reviews (cascade delete)

A module contains multiple pieces of content (cascade delete)

A quiz contains multiple questions (cascade delete)

Students can write reviews for courses (restrict delete to avoid circular cascade)

Many-to-many relationships:

Students and courses via StudentCourse

Students and module contents via ContentProgress

Seed Data
The database includes sample data for students, teachers, courses, modules, and content to support testing and demonstration.
