using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockManagementWebApplication.Data;
using StockManagementWebApplication.Enums;
using StockManagementWebApplication.Models.DTO;
using System.Security.Claims;

namespace StockManagementWebApplication.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly StockManagementDbContext _context;
        public OrderController(StockManagementDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Order(int OrderId)
        {
            var data = _context.Orders.Where(a => a.Id == OrderId).FirstOrDefault();
            if (data == null)
            {
                return NotFound();
            }
            data.Status = (int)OrderStatusEnum.Processing;
            await _context.SaveChangesAsync();
            return RedirectToAction("MyOrder");
        }

        public async Task<IActionResult> MyOrder()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
            var data = _context.Orders.Where(a => a.OrderdBy == userId
                                    && a.Status != (int)OrderStatusEnum.Carted)
                                .Include(k => k.OrderItems)
                                .Include("OrderItems.Item")
                                .Select(j => new OrderDTO()
                                {
                                    OrderId = j.Id,
                                    StatusId = j.Status,
                                    OrderDate = j.OrderDate,
                                    Items = j.OrderItems.Select(a => new OrderItemDTO()
                                    {
                                        Id = a.Id,
                                        Name = a.Item.Name,
                                        Quantity = a.Quantity,
                                        Rate = a.Rate
                                    }).ToList()
                                }).OrderByDescending(a => a.OrderDate).AsEnumerable();

            data = data.Select(i =>
            new OrderDTO()
            {
                Items = i.Items,
                OrderId = i.OrderId,
                OrderDate = i.OrderDate,
                StatusId = i.StatusId,
                Status = ((OrderStatusEnum)Enum.Parse(typeof(OrderStatusEnum), i.StatusId.ToString())).ToString(),

            });
            if (data == null)
            {
                return NotFound();
            }
            return View(data);
        }


        public async Task<IActionResult> ModifyOrderItem(int id)
        {
            var data = _context.OrderItems.Where(a => a.Id == id && a.Order.Status == (int)OrderStatusEnum.Processing)
                .Include(k => k.Order)
                .Select(a => new OrderItemDTO()
                {
                    Id = a.Id,
                    Name = a.Item.Name,
                    Quantity = a.Quantity,
                    Rate = a.Rate
                }).FirstOrDefault();
            if (data == null)
            {
                return NotFound();
            }
            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> ModifyOrderItem(OrderItemDTO item)
        {
            var data = await _context.OrderItems.Where(a => a.Id == item.Id)
                        .FirstOrDefaultAsync();
            if (data == null)
            {
                return NotFound();
            }
            data.Quantity = item.Quantity;
            await _context.SaveChangesAsync();
            return RedirectToAction("MyOrder");

        }

        public async Task<IActionResult> RemoveOrderItem(int id)
        {
            var data = _context.OrderItems.Where(a => a.Id == id && a.Order.Status == (int)OrderStatusEnum.Processing)
                .Include(k => k.Order)
                .Select(a => new OrderItemDTO()
                {
                    Id = a.Id,
                    Name = a.Item.Name,
                    Quantity = a.Quantity,
                    Rate = a.Rate
                }).FirstOrDefault();
            if (data == null)
            {
                return NotFound();
            }
            return View(data);
        }



        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Items == null)
            {
                return Problem("Entity set 'StockManagementDbContext.Items'  is null.");
            }
            var item = await _context.OrderItems.FindAsync(id);
            if (item != null)
            {
                _context.OrderItems.Remove(item);
                var order = await _context.Orders.FindAsync(item.OrderId);
                bool hasItems = order.OrderItems.Any(a => a.Id != id);
                if (!hasItems)
                {
                    _context.Orders.Remove(order);
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("MyOrder");
        }


        public async Task<IActionResult> Cancel(int id)
        {
            var data = _context.Orders.Where(a => a.Id == id&& a.Status == (int)OrderStatusEnum.Processing)
                .Select(a => new OrderDTO()
                {
                    OrderId = a.Id,

                    Items = a.OrderItems.Select(a => new OrderItemDTO()
                    {
                        Id = a.Id,
                        Name = a.Item.Name,
                        Quantity = a.Quantity,
                        Rate = a.Rate
                    }).ToList()
                }).FirstOrDefault();
            if (data == null)
            {
                return NotFound();
            }
            return View(data);
        }



        [HttpPost, ActionName("Cancel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(int id)
        {
            if (_context.Items == null)
            {
                return Problem("Entity set 'StockManagementDbContext.Items'  is null.");
            }
            var item = await _context.Orders.FindAsync(id);
            if (item != null)
            {
                item.Status = (int)OrderStatusEnum.Cancelled;
            }

            await _context.SaveChangesAsync();
            if (this.User.IsInRole(RoleEnum.Manager.ToString()))
            {
                return RedirectToAction("All");

            }
            else
            {
                return RedirectToAction("MyOrder");

            }
        }




        public async Task<IActionResult> All()
        {
            var data = _context.Orders.Where(a => a.Status == (int)OrderStatusEnum.Processing)
                                .Include(k => k.OrderItems)
                                .Include("OrderItems.Item")
                                .Select(j => new OrderDTO()
                                {
                                    OrderId = j.Id,
                                    StatusId = j.Status,
                                    OrderDate = j.OrderDate,
                                    Items = j.OrderItems.Select(a => new OrderItemDTO()
                                    {
                                        Id = a.Id,
                                        Name = a.Item.Name,
                                        Quantity = a.Quantity,
                                        Rate = a.Rate
                                    }).ToList()
                                }).OrderByDescending(a => a.OrderDate).AsEnumerable();

            data = data.Select(i =>
            new OrderDTO()
            {
                Items = i.Items,
                OrderId = i.OrderId,
                OrderDate = i.OrderDate,
                StatusId = i.StatusId,
                Status = ((OrderStatusEnum)Enum.Parse(typeof(OrderStatusEnum), i.StatusId.ToString())).ToString(),

            });
            if (data == null)
            {
                return NotFound();
            }
            return View("MyOrder", data);
        }

        public async Task<IActionResult> Process(int id)
        {
            var data = _context.Orders.Where(a => a.Id == id)
                .Select(a => new OrderDTO()
                {
                    OrderId = a.Id,

                    Items = a.OrderItems.Select(a => new OrderItemDTO()
                    {
                        Id = a.Id,
                        Name = a.Item.Name,
                        Quantity = a.Quantity,
                        Rate = a.Rate
                    }).ToList()
                }).FirstOrDefault();
            if (data == null)
            {
                return NotFound();
            }
            return View(data);
        }


        [HttpPost, ActionName("Process")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessConfirmed(int id)
        {
            if (_context.Items == null)
            {
                return Problem("Entity set 'StockManagementDbContext.Items'  is null.");
            }
            var item = await _context.Orders.FindAsync(id);
            if (item != null)
            {
                item.Status = (int)OrderStatusEnum.Deliverd;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("All");
        }


    }
}
