using EnergyMonitor.MVC.Helpers;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace EnergyMonitor.MVC.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApiHelper _apiHelper;

        public AuthController(ApiHelper apiHelper)
        {
            _apiHelper = apiHelper;
        }

        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("UserEmail") != null)
                return RedirectToAction("Index", "Dashboard");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(
            string email, string password)
        {
            var result = await _apiHelper.PostAsync<Dictionary<string, object>>(
                "/api/Auth/login",
                new { email, password });

            if (result == null)
            {
                ViewBag.Error = "Invalid email or password";
                return View();
            }

            HttpContext.Session.SetString("UserEmail",
                result["email"].ToString() ?? "");
            HttpContext.Session.SetString("UserName",
                result["full_Name"].ToString() ?? "");
            HttpContext.Session.SetString("UserRole",
                result["role"].ToString() ?? "");

            return RedirectToAction("Index", "Dashboard");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}