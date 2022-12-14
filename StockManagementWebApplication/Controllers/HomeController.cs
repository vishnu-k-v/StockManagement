using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockManagementWebApplication.Data;
using StockManagementWebApplication.Enums;
using StockManagementWebApplication.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace StockManagementWebApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly StockManagementDbContext _stockManagementDbContext;
        public HomeController(ILogger<HomeController> logger,
            StockManagementDbContext stockManagementDbContext)
        {
            _logger = logger;
            _stockManagementDbContext = stockManagementDbContext;
        }

        /// <summary>
        /// Home Page For Every User
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            var userRoles = User.Claims.Where(a => a.Type == ClaimTypes.Role)
                            .Select(a => a.Value).ToList();
            if (userRoles.Contains(RoleEnum.Manager.ToString()))
            {
                return RedirectToAction("All", "Order");
            }

            var tenDaysBefore = DateTime.Now.AddDays(-10);
            var data = _stockManagementDbContext.OrderItems
                        .Include(i => i.Item)
                        .Where(i => i.Order.OrderDate >= tenDaysBefore)
                        .GroupBy(a => a.ItemId)
                        .Select(s => new
                        {
                            id = s.Key,
                            Item = s.First().Item,
                            count = s.Count()
                        })
                        .OrderBy(p => p.count)
                        .Take(10).AsEnumerable()
                        .Select(o => o.Item)
                        .ToList();

            return View(data);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}