using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockManagementWebApplication.Data;
using StockManagementWebApplication.Enums;
using StockManagementWebApplication.Models.DTO;
using System.Security.Claims;

namespace StockManagementWebApplication.Controllers
{
    /// <summary>
    /// Control All Order,Cancel and Approve 
    /// </summary>
    [Authorize]
    public class OrderController : Controller
    {
        private readonly StockManagementDbContext _context;
        public OrderController(StockManagementDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Order The carted Items
        /// </summary>
        /// <param name="OrderId"></param>
        /// <returns></returns>
        public async Task<IActionResult> Order(int OrderId)
        {
            var data = _context.Orders.Where(a => a.Id == OrderId)
                        .Include(k => k.OrderItems)
                        .Include("OrderItems.Item")
                        .FirstOrDefault();
            if (data == null)
            {
                return NotFound();
            }
            foreach (var item in data.OrderItems)
            {
                if (item.Item.Quantity < item.Quantity)
                {
                    var errorData = new OutOfStockDTO()
                    {
                        AvailableQuantity = item.Item.Quantity,
                        Name = item.Item.Name
                    };
                    return View("OutOfStock", errorData);
                }
                item.Item.Quantity -= item.Quantity;
            }
            
            data.Status = (int)OrderStatusEnum.Processing;
            await _context.SaveChangesAsync();
            return RedirectToAction("MyOrder");
        }

        /// <summary>
        /// List Dopwn All the orders of the logined in customer
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// return modify order item view
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Modify an item in the order
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ModifyOrderItem(OrderItemDTO item)
        {
            var data = await _context.OrderItems.Where(a => a.Id == item.Id)
                            .Include(j => j.Item)
                            .FirstOrDefaultAsync();
            if (data == null)
            {
                return NotFound();
            }
            data.Item.Quantity += data.Quantity;
            if (data.Item.Quantity < item.Quantity)
            {
                var errorData = new OutOfStockDTO()
                {
                    AvailableQuantity = data.Item.Quantity,
                    Name = data.Item.Name
                };
                return View("OutOfStock", errorData); 
            }
            data.Quantity = item.Quantity;
            data.Item.Quantity -= item.Quantity;
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
            var item = await _context.OrderItems.Where(h => h.Id == id)
                            .Include(a => a.Item).FirstOrDefaultAsync();
            if (item != null)
            {
                item.Item.Quantity += item.Quantity;
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
            var data = _context.Orders.Where(a => a.Id == id && a.Status == (int)OrderStatusEnum.Processing)
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
            var item = _context.Orders.Where(a => a.Id == id)
                        .Include(k => k.OrderItems)
                        .Include("OrderItems.Item")
                        .FirstOrDefault();
            if (item != null)
            {
                foreach (var it in item.OrderItems)
                {
                    it.Item.Quantity += it.Quantity;
                }
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



        /// <summary>
        /// get All Orders For Manager
        /// </summary>
        /// <returns></returns>
        /// 

        [Authorize(Roles = "Manager")]
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

        /// <summary>
        /// return process confirmation page
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// 
        [Authorize(Roles = "Manager")]
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

        /// <summary>
        /// Make it as deliverd order by manager
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost, ActionName("Process")]
        [Authorize(Roles = "Manager")]
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
                item.Status = (int)OrderStatusEnum.Delivered ;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("All");
        }


    }
}
