using EduGate.Data;
using EduGate.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;

    public AccountController(ApplicationDbContext context)
    {
        _context = context;
    }



    // Improved GetLoggedInUser method with all necessary includes for Dashboard
    private IAccount GetLoggedInUser()
    {
        string userType = HttpContext.Session.GetString("UserType");

        if (userType == "Student")
        {
            int? studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId.HasValue)
            {
                return _context.Students
                    .Include(s => s.EnrolledCourses)
                        .ThenInclude(ec => ec.Course)
                            .ThenInclude(c => c.Modules)
                                .ThenInclude(m => m.Contents)
                    .Include(s => s.ContentProgresses)
                    .FirstOrDefault(s => s.Id == studentId.Value);
            }
        }
        else if (userType == "Teacher")
        {
            int? teacherId = HttpContext.Session.GetInt32("TeacherId");
            if (teacherId.HasValue)
            {
                return _context.Teachers
                    .Include(t => t.UploadedCourses)
                        .ThenInclude(c => c.Modules)
                    .FirstOrDefault(t => t.Id == teacherId.Value);
            }
        }

        return null;
    }

    [HttpGet]
    public IActionResult Login(string returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    [HttpPost]
    public IActionResult Login(string email, string password, string returnUrl = null)
    {
        var student = _context.Students.FirstOrDefault(s => s.Email == email && s.Password == password);
        var teacher = _context.Teachers.FirstOrDefault(t => t.Email == email && t.Password == password);

        if (student != null)
        {
            // Store student ID in session
            HttpContext.Session.SetInt32("StudentId", student.Id);
            HttpContext.Session.SetString("UserType", "Student");
            HttpContext.Session.SetString("UserName", student.Name);

            // Still keep TempData for backward compatibility
            TempData["UserName"] = student.Name;

            // Redirect to returnUrl if provided, otherwise go to home
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            else
                return RedirectToAction("Index", "Home");
        }
        else if (teacher != null)
        {
            // Store teacher ID in session
            HttpContext.Session.SetInt32("TeacherId", teacher.Id);
            HttpContext.Session.SetString("UserType", "Teacher");
            HttpContext.Session.SetString("UserName", teacher.Name);

            // Still keep TempData for backward compatibility
            TempData["UserName"] = teacher.Name;

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            else
                return RedirectToAction("Index", "Home");
        }

        ViewBag.Error = "Invalid email or password.";
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }
    // Add a logout action
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }

    // Update your Register methods to also set session
    [HttpPost]
    public IActionResult Register(Student student)
    {
        if (ModelState.IsValid)
        {
            _context.Students.Add(student);
            _context.SaveChanges();

            // Set session variables after registration
            HttpContext.Session.SetInt32("StudentId", student.Id);
            HttpContext.Session.SetString("UserType", "Student");
            HttpContext.Session.SetString("UserName", student.Name);

            TempData["UserName"] = student.Name;
            return RedirectToAction("Index", "Home");
        }
        return View(student);
    }

    [HttpPost]
    public IActionResult RegisterTeacher(Teacher teacher)
    {
        if (ModelState.IsValid)
        {
            _context.Teachers.Add(teacher);
            _context.SaveChanges();

            // Set session variables after registration
            HttpContext.Session.SetInt32("TeacherId", teacher.Id);
            HttpContext.Session.SetString("UserType", "Teacher");
            HttpContext.Session.SetString("UserName", teacher.Name);

            TempData["UserName"] = teacher.Name;
            return RedirectToAction("Index", "Home");
        }
        return View(teacher);
    }
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpGet]
    public IActionResult RegisterTeacher()
    {
        return View();
    }
    public IActionResult Profile()
    {
        var user = GetLoggedInUser();
        if (user == null)
        {
            return RedirectToAction("Login");
        }
        return View(user);
    }

    public IActionResult Dashboard()
    {
        var user = GetLoggedInUser();
        if (user == null)
        {
            return RedirectToAction("Login");
        }
        return View(user);
    }
    [HttpPost]
    public IActionResult UpdateProfile(string email, string password)
    {
        string userType = HttpContext.Session.GetString("UserType");

        if (userType == "Student")
        {
            int? studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId.HasValue)
            {
                var student = _context.Students.Find(studentId.Value);
                if (student != null)
                {
                    student.Email = email;
                    if (!string.IsNullOrEmpty(password))
                    {
                        student.Password = password; // You should use proper hashing here
                    }
                    _context.SaveChanges();
                }
            }
        }
        else if (userType == "Teacher")
        {
            int? teacherId = HttpContext.Session.GetInt32("TeacherId");
            if (teacherId.HasValue)
            {
                var teacher = _context.Teachers.Find(teacherId.Value);
                if (teacher != null)
                {
                    teacher.Email = email;
                    if (!string.IsNullOrEmpty(password))
                    {
                        teacher.Password = password; // You should use proper hashing here
                    }
                    _context.SaveChanges();
                }
            }
        }

        return RedirectToAction("Profile");
    }
}