using freshstore.bll;
using freshstore.bll.Consts;
using freshstore.bll.Dtos;
using freshstore.bll.Extensions;
using freshstore.bll.Models;
using freshstore.Requests.Order;
using freshstore.Responses.Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace freshstore.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{v:apiVersion}/orders")]
    public partial class OrdersController : ControllerBase
    {
        private readonly FreshContext _context;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(FreshContext context, ILogger<OrdersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // user route
        [HttpGet]
        [Authorize(Policy = UserPermissionConsts.CAN_VIEW_ORDERS)]
        public async Task<ActionResult<IEnumerable<GetOrderResponse>>> Get(long pageSize = 10, long pageNo = 0)
        {
            string identifier = Request.GetUserEmail();

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == identifier && x.IsDeleted == false);
            if (user == null)
            {
                return BadRequest("User not found");
            }

            if (pageSize == 0) pageSize = 10;
            int skip = Convert.ToInt32(pageSize * pageNo);

            var orders = await _context.Orders
                            .Include(x => x.OrderItems)
                            .ThenInclude(x => x.Product)
                            .OrderByDescending(x => x.CreatedOn)
                            .Where(x => x.UserId == user.Id)
                            .Select(x => new GetOrderResponse
                            {
                                Id = x.Id,
                                PlacedOn = x.CreatedOn,
                                OrderItems = x.OrderItems.Select(x => new OrderItemModel
                                {
                                    UnitPrice = x.UnitPrice,
                                    Quantity = x.Quantity,
                                    OrderId = x.OrderId,
                                    ProductId = x.ProductId,
                                    Product = new ProductModel
                                    {
                                        Id = x.Product.Id,
                                        Name = x.Product.Name,
                                        UnitPrice = x.Product.UnitPrice,
                                    }
                                }).ToList()
                            })
                            .Skip(skip)
                            .Take((int)pageSize)
                            .ToListAsync();

            return Ok(orders);
        }

        // user route
        [HttpGet("{id}")]
        [Authorize(Policy = UserPermissionConsts.CAN_VIEW_ORDERS)]
        public async Task<ActionResult<GetOrderResponse>> Get(int id)
        {
            string identifier = Request.GetUserEmail();

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == identifier && x.IsDeleted == false);
            if (user == null)
            {
                return BadRequest("User not found");
            }

            var order = await _context.Orders
                            .Where(x => x.Id == id)
                            .Where(x => x.UserId == user.Id)
                            .Include(x => x.OrderItems)
                            .ThenInclude(x => x.Product)
                            .OrderByDescending(x => x.CreatedOn)
                            .Select(x => new GetOrderResponse
                            {
                                OrderItems = x.OrderItems.Select(x => new OrderItemModel
                                {
                                    UnitPrice = x.UnitPrice,
                                    Quantity = x.Quantity,
                                    OrderId = x.OrderId,
                                    ProductId = x.ProductId,
                                    Product = new ProductModel
                                    {
                                        Id = x.Product.Id,
                                        Name = x.Product.Name,
                                        UnitPrice = x.Product.UnitPrice,
                                    }
                                }).ToList()
                            })
                            .AsNoTracking()
                            .FirstOrDefaultAsync();

            return Ok(order);
        }

        [HttpPost]
        [Route("placeorder")]
        [Authorize(Policy = UserPermissionConsts.CAN_PLACE_ORDER)]
        public async Task<ActionResult<GetOrderResponse>> PlaceOrderFromBasket()
        {
            string identifier = Request.GetUserEmail();

            //string identifier = Request.HttpContext?.User?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value ?? String.Empty;
            //if (string.IsNullOrEmpty(identifier))
            //{
            //    return BadRequest("Failed to);
            //}

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == identifier && x.IsDeleted == false);
            if (user == null)
            {
                return BadRequest("User not found");
            }

            var basket = await _context.Baskets
                                .Include(x => x.BasketItems)
                                .FirstOrDefaultAsync(x => x.UserId == user.Id);
            if (basket == null)
            {
                // change message here
                return BadRequest("Basket not found !");
            }

            // basket has items
            if (basket.BasketItems.Count == 0)
            {
                _logger.LogError("No basket items found");
                // log error message
                return Ok(new ResponseMessage { Success = false, Message = "Failed to place an order, please contact your system administrator.", RequestRefresh = true });
            }

            // basket items have quantity greater than 0
            var validBasketItems = basket.BasketItems.Where(x => x.Quantity > 0).ToList();
            if (validBasketItems.Count == 0)
            {
                _logger.LogError("Basket items contained items with zero quantity, they're ignored.");
                // log error message
                return Ok(new ResponseMessage { Success = false, Message = "Failed to place an order, please contact your system administrator.", RequestRefresh = true });
            }

            // getting list of Product to ensure, latest pricing
            var ProductIds = validBasketItems.Select(x => x.ProductId).ToList();
            var Products = await _context.Products.Where(x => ProductIds.Contains(x.Id)).AsNoTracking().ToListAsync();

            var order = new Order();
            order.UserId = user.Id;
            order.OrderItems = new List<OrderItem>();

            foreach (var basketItem in validBasketItems)
            {
                var Product = Products.FirstOrDefault(x => x.Id == basketItem.ProductId);
                if (Product == null)
                {
                    _logger.LogError("Product in the basket item was not found");
                    // log here
                    continue;
                }
                var orderItem = new OrderItem { ProductId = basketItem.ProductId, Quantity = basketItem.Quantity, UnitPrice = Product.UnitPrice };
                order.OrderItems.Add(orderItem);
            }
            
            if (order.OrderItems.Count == 0)
            {
                // log error message
                return Ok(new ResponseMessage { Success = false, Message = "Failed to place an order, please contact your system administrator." });
            }

            // adding order
            _context.Orders.Add(order);
            // removing basket
            _context.Baskets.Remove(basket);

            int rowsAffected = await _context.SaveChangesAsync();

            if (rowsAffected == 0)
            {
                _logger.LogCritical($"Order could NOT be placed");

                return Ok(new ResponseMessage { Success = false, Message = $"Failed to place order, please try again or contact system administrator." });
            }

            return Created(new Uri($"{Request.Path}", UriKind.Relative), new ResponseMessage { Success = true, Message = "Order has been placed" });
        }


        [HttpPost]
        [Route("placeorderwithdata")]
        [Authorize(Policy = UserPermissionConsts.CAN_PLACE_ORDER)]
        public async Task<ActionResult<GetOrderResponse>> PlaceOrderDirectly([FromBody] PlaceDirectOrderRequest model)
        {
            if(!ModelState.IsValid)
            {
                ModelState.AddModelError(String.Empty, "Items can't be empty.");
                return BadRequest(ModelState);
            }

            string identifier = Request.GetUserEmail();

            //string identifier = Request.HttpContext?.User?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value ?? String.Empty;
            //if (string.IsNullOrEmpty(identifier))
            //{
            //    return BadRequest("Failed to);
            //}

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == identifier && x.IsDeleted == false);
            if (user == null)
            {
                return BadRequest("User not found");
            }

            //var basket = await _context.Baskets
            //                    .Include(x => x.BasketItems)
            //                    .FirstOrDefaultAsync(x => x.UserId == user.Id);
            //if (basket == null)
            //{
            //    // change message here
            //    return BadRequest("Basket not found !");
            //}

            //// basket has items
            //if (basket.BasketItems.Count == 0)
            //{
            //    _logger.LogError("No basket items found");
            //    // log error message
            //    return Ok(new ResponseMessage { Success = false, Message = "Failed to place an order, please contact your system administrator.", RequestRefresh = true });
            //}

            // basket items have quantity greater than 0
            var validBasketItems = model.Items.Where(x => x.Quantity > 0).ToList();
            if (validBasketItems.Count == 0)
            {
                _logger.LogError("Items contained items with zero quantity, they're ignored.");
                // log error message
                return Ok(new ResponseMessage { Success = false, Message = "Failed to place an order, please contact your system administrator.", RequestRefresh = true });
            }

            // getting list of Product to ensure, latest pricing
            var productIds = validBasketItems.Select(x => x.ProductId).ToList();
            var products = await _context.Products.Where(x => productIds.Contains(x.Id)).AsNoTracking().ToListAsync();

            var order = new Order();
            order.UserId = user.Id;
            order.OrderItems = new List<OrderItem>();

            foreach (var item in model.Items)
            {
                var product = products.FirstOrDefault(x => x.Id == item.ProductId);
                if (product == null)
                {
                    _logger.LogError("Product was not found");
                    // log here
                    continue;
                }
                var orderItem = new OrderItem { ProductId = item.ProductId, Quantity = item.Quantity, UnitPrice = product.UnitPrice };
                order.OrderItems.Add(orderItem);
            }

            if (order.OrderItems.Count == 0)
            {
                // log error message
                return Ok(new ResponseMessage { Success = false, Message = "Failed to place an order, please contact your system administrator." });
            }

            // adding order
            _context.Orders.Add(order);

            int rowsAffected = await _context.SaveChangesAsync();

            if (rowsAffected == 0)
            {
                _logger.LogCritical($"Order could NOT be placed");

                return Ok(new ResponseMessage { Success = false, Message = $"Failed to place order, please try again or contact system administrator." });
            }

            return Created(new Uri($"{Request.Path}", UriKind.Relative), new ResponseMessage { Success = true, Message = "Order has been placed" });
        }

        //[HttpDelete]
        //[Route("{id}")]
        ////[Authorize(Policy = UserPermissionConsts.CanUpdateAnOrder)]
        //public async Task<ActionResult> Delete(int id)
        //{
        //    var order = _context.Orders.Find(id);
        //    if (order == null)
        //    {
        //        return BadRequest(Messages.NOT_FOUND_MESSAGE);
        //    }

        //    if (order.IsDeleted)
        //    {
        //        string message = $"Order could NOT be deleted with the Id {id}, because it was already deleted.";
        //        _logger.LogWarning(message);
        //        return Ok(new ResponseMessage { Success = true, Message = message });
        //    }

        //    order.IsDeleted = true;
        //    int rowsAffected = await _context.SaveChangesAsync();
        //    if (rowsAffected == 0)
        //    {

        //        _logger.LogCritical($"Order could NOT be deleted with the Id {id}");

        //        return Ok(new ResponseMessage { Success = false, Message = $"Order with the Id `{id}` could NOT be deleted, please try again or contact system administrator." });
        //    }

        //    return Ok(new ResponseMessage { Success = true, Message = $"Order with the Id `{order.Id}` has been marked as deleted." });
        //}
    }
}
