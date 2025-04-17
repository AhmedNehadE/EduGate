using Microsoft.AspNetCore.Mvc;
using EduGate.Data;
using Microsoft.AspNetCore.Http;

namespace EduGate.ViewComponents
{
    public class UserInfoViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserInfoViewComponent(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public IViewComponentResult Invoke()
        {
            var userType = _httpContextAccessor.HttpContext.Session.GetString("UserType");
            var userName = _httpContextAccessor.HttpContext.Session.GetString("UserName");

            var viewModel = new UserInfoViewModel
            {
                UserName = userName,
                UserType = userType,
                IsLoggedIn = !string.IsNullOrEmpty(userName)
            };

            return View(viewModel);
        }
    }

    public class UserInfoViewModel
    {
        public string UserName { get; set; }
        public string UserType { get; set; }
        public bool IsLoggedIn { get; set; }
    }
}