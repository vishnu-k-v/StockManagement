using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockManagementWebApplication.Data;
using StockManagementWebApplication.Enums;
using StockManagementWebApplication.Models.DTO;
using StockManagementWebApplication.Models.Entities;
using System.Security.Claims;

namespace StockManagementWebApplication.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CartController : Controller
    {
        private readonly StockManagementDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(StockManagementDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
        }

        // GET: CartController
        public ActionResult Index()
        {
            var userEmasil = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var data = _context.Orders.Where(a => a.Status == (int)OrderStatusEnum.Carted
                                            && a.OrderdBy == userEmasil)

                                .Include(k => k.OrderItems)
                                .Include("OrderItems.Item")
                                .Select(r => new OrderDTO()
                                {
                                    OrderId = r.Id,
                                    Items = r.OrderItems.Select(a => new OrderItemDTO()
                                    {
                                        Id = a.Id,
                                        Name = a.Item.Name,
                                        Quantity = a.Quantity,
                                        Rate = a.Rate
                                    }).ToList()
                                })
                                .FirstOrDefault();
           

            return View(data);

        }

        public async Task<IActionResult> Add(int? id)
        {
            if (id == null || _context.Items == null)
            {
                return NotFound();
            }

            var item = await _context.Items.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }
            item.Quantity = 0;
            return View(item);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(CreateOrderDTO orderItem)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
            if (ModelState.IsValid)
            {
                var data = _context.Orders
                                    .Where(a => a.Status == (int)OrderStatusEnum.Carted
                                           && a.OrderdBy == userId)
                                    .Include(k => k.OrderItems)
                                    .Select(a => a).FirstOrDefault();
                OrderItem item = new()
                {
                    Quantity = orderItem.Quantity,
                    Rate = orderItem.Rate,
                    ItemId = orderItem.Id,
                };
                if (data != null)
                {
                    var alreadyExistItem = data.OrderItems.FirstOrDefault(a => a.ItemId == orderItem.Id);
                    if (alreadyExistItem != null)
                    {
                        alreadyExistItem.Quantity += orderItem.Quantity;
                    }
                    else
                    {
                        data.OrderItems.Add(item);
                    }
                }
                else
                {
                    Order order = new()
                    {
                        OrderdBy = userId,
                        OrderDate = DateTime.Now,
                        Status = (int)OrderStatusEnum.Carted,
                    };
                    var items = new List<OrderItem>();
                    items.Add(item);
                    order.OrderItems = items;
                    _context.Orders.Add(order);
                }


                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index","Items");

        }



    }
}
