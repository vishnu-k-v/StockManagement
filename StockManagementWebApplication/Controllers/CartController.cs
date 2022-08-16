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
    /// <summary>
    /// Cart Controller 
    /// Add and Modify Cart
    /// </summary>
    [Authorize(Roles = "Customer")]
    public class CartController : Controller
    {
        private readonly StockManagementDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(StockManagementDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
        }

        /// <summary>
        /// Get carted Items For current user
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Add To Cart View
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Add To Cart Submit Action
        /// </summary>
        /// <param name="orderItem"></param>
        /// <returns></returns>
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
            return RedirectToAction("Index", "Items");

        }

        /// <summary>
        /// Modify Cart item View
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Modify(int? id)
        {
            if (id == null || _context.Items == null)
            {
                return NotFound();
            }

            var item = await _context.OrderItems
                        .Include(a => a.Item).Where(h => h.Id == id)
                        .Select(s => new ModifyCartDTO()
                        {
                            ItemId = s.ItemId,
                            OrderId = s.OrderId,
                            Name = s.Item.Name,
                            OrderItemId = s.Id,
                            Quantity = s.Quantity,
                            Rate = s.Item.Rate
                        })
                        .FirstOrDefaultAsync();
            if (item == null)
            {
                return NotFound();
            }
            return View(item);
        }

        /// <summary>
        /// Modify cart Item ,Change Quantity
        /// </summary>
        /// <param name="modifyCartDTO"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ModifyCart(ModifyCartDTO modifyCartDTO)
        {
            var orderItem = await _context.OrderItems.Where(a => a.Id == modifyCartDTO.OrderItemId)
                            .FirstOrDefaultAsync();
            if (orderItem == null)
            {
                return NotFound();
            }
            if (modifyCartDTO.Quantity == 0)
            {
                var order = await _context.Orders.FindAsync(orderItem.OrderId);
                bool hasItems = order.OrderItems.Any(a => a.Id != modifyCartDTO.OrderItemId);
                if (!hasItems)
                {
                    _context.Orders.Remove(order);
                }
                _context.Remove(orderItem);
            }
            else
            {
                orderItem.Quantity = modifyCartDTO.Quantity;
                orderItem.Rate = modifyCartDTO.Rate;
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
