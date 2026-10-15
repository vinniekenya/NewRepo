using Microsoft.AspNetCore.Mvc;
using TradingSignalsApp.Data;
using TradingSignalsApp.Models;
using System.Linq;

namespace TradingSignalsApp.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ CHECK IF ADMIN
        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("UserRole") == "Admin";
        }

        // ✅ VIEW ALL USERS
        public IActionResult Index()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var users = _context.Users.ToList();
            return View(users);
        }

        // ✅ APPROVE USER
        public IActionResult Approve(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var user = _context.Users.Find(id);
            if (user != null)
            {
                user.IsApproved = true;
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        // ✅ REJECT USER
        public IActionResult Reject(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var user = _context.Users.Find(id);
            if (user != null)
            {
                user.IsApproved = false;
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        // ✅ MARK AS VIP
        public IActionResult MakeVip(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var user = _context.Users.Find(id);
            if (user != null)
            {
                user.HasPaid = true;
                user.Role = "VIP";
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        // ✅ REMOVE VIP
        public IActionResult RemoveVip(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var user = _context.Users.Find(id);
            if (user != null)
            {
                user.HasPaid = false;
                user.Role = "User";
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        // ✅ DELETE USER
        public IActionResult Delete(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var user = _context.Users.Find(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}