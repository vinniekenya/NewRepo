using Microsoft.AspNetCore.Mvc;
using TradingSignalsApp.Data;
using TradingSignalsApp.Models;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace TradingSignalsApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ HASH PASSWORD
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        // ✅ VERIFY PASSWORD
        private bool VerifyPassword(string password, string hashedPassword)
        {
            return HashPassword(password) == hashedPassword;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            // ✅ Hardcoded admin — returns immediately
            if (email == "admin@mail.com" && password == "admin123")
            {
                HttpContext.Session.SetString("UserEmail", "admin@mail.com");
                HttpContext.Session.SetString("UserRole", "Admin");
                HttpContext.Session.SetString("Username", "Admin");
                return RedirectToAction("Index", "Dashboard");
            }

            try
            {
                // ✅ Find user by email only
                var user = _context.Users
                    .FirstOrDefault(u => u.Email == email);

                if (user == null)
                {
                    ViewBag.Error = "Invalid email or password";
                    return View();
                }

                // ✅ Verify hashed password
                if (!VerifyPassword(password, user.Password ?? ""))
                {
                    ViewBag.Error = "Invalid email or password";
                    return View();
                }

                if (!user.IsApproved)
                {
                    ViewBag.Error = "Your account is pending admin approval";
                    return View();
                }

                HttpContext.Session.SetString("UserEmail", user.Email ?? "");
                HttpContext.Session.SetString("UserRole", user.Role ?? "User");
                HttpContext.Session.SetString("Username", user.Username ?? "");

                return RedirectToAction("Index", "Dashboard");
            }
            catch
            {
                ViewBag.Error = "Database error. Please try again.";
                return View();
            }
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string username, string email, string password)
        {
            try
            {
                var existing = _context.Users
                    .FirstOrDefault(u => u.Email == email);

                if (existing != null)
                {
                    ViewBag.Error = "Email already registered";
                    return View();
                }

                var newUser = new User
                {
                    Username = username,
                    Email = email,
                    Password = HashPassword(password), // ✅ hashed
                    Role = "User",
                    IsApproved = false,
                    HasPaid = false,
                    CreatedAt = DateTime.Now
                };

                _context.Users.Add(newUser);
                _context.SaveChanges();

                ViewBag.Success = "Registration successful! Please wait for admin approval.";
                return View();
            }
            catch
            {
                ViewBag.Error = "Database error. Please try again.";
                return View();
            }
        }

        // ✅ UPGRADE GET
        public IActionResult Upgrade()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserEmail")))
                return RedirectToAction("Login", "Account");

            return View();
        }

        // ✅ UPGRADE POST
        [HttpPost]
        public IActionResult Upgrade(string mpesaCode)
        {
            if (string.IsNullOrEmpty(mpesaCode))
            {
                ViewBag.Error = "Please enter your M-Pesa transaction code";
                return View();
            }

            try
            {
                var email = HttpContext.Session.GetString("UserEmail");
                var user = _context.Users.FirstOrDefault(u => u.Email == email);

                if (user != null)
                {
                    user.Username = $"{user.Username} | MPESA: {mpesaCode}";
                    _context.SaveChanges();
                }
            }
            catch { }

            ViewBag.Success = "Payment submitted! Your account will be upgraded to VIP within 24 hours after admin verification.";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
    }
}