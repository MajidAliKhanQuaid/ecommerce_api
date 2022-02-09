using freshstore.bll;
using freshstore.bll.Consts;
using freshstore.bll.Dtos;
using freshstore.bll.Extensions;
using freshstore.bll.Models;
using freshstore.Requests.Basket;
using freshstore.Responses.Basket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace freshstore.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{v:apiVersion}/baskets")]
    public partial class BasketsController : ControllerBase
    {
        private readonly FreshContext _context;
        private readonly ILogger<BasketsController> _logger;

        public BasketsController(FreshContext context, ILogger<BasketsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Policy = UserPermissionConsts.CAN_USE_BASKET)]
        public async Task<ActionResult<BasketResponse>> Get()
        {
            string identifier = Request.GetUserEmail();

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == identifier && x.IsDeleted == false);
            if (user == null)
            {
                return BadRequest("User not found");
            }

            var basket = await _context.Baskets
                            .Include(x => x.BasketItems)
                            .ThenInclude(x => x.Product)
                            .OrderByDescending(x => x.CreatedOn)
                            .Where(x => x.UserId == user.Id)
                            .Select(x => new BasketResponse
                            {
                                Total = x.BasketItems.Sum(x => x.UnitPrice * x.Quantity),
                                ItemsCount = x.BasketItems.Sum(x => x.Quantity),
                                Discount = 0,
                                BasketItems = x.BasketItems.Select(x => new BasketItemModel
                                {
                                    UnitPrice = x.UnitPrice,
                                    Quantity = x.Quantity,
                                    BasketId = x.BasketId,
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

            if (basket != null)
            {

            }

            return Ok(basket);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BasketResponse>> Get(int id)
        {
            var basket = await _context.Baskets
                            .Include(x => x.BasketItems)
                            .ThenInclude(x => x.Product)
                            .OrderByDescending(x => x.CreatedOn)
                            .Select(x => new BasketResponse
                            {
                                BasketItems = x.BasketItems.Select(x => new BasketItemModel
                                {
                                    UnitPrice = x.UnitPrice,
                                    Quantity = x.Quantity,
                                    BasketId = x.BasketId,
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
                            .ToListAsync();

            return Ok(basket);
        }

        [HttpPost("update")]
        [Authorize(Policy = UserPermissionConsts.CAN_USE_BASKET)]
        public async Task<ActionResult> AddOrUpdateItem([FromBody] UpdateBasketItemRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var product = await _context.Products.FindAsync(model.ProductId);
            if (product == null)
            {
                // log here to check if
                return BadRequest(Messages.INSERT_ERROR);
            }

            // since the prices have changes
            bool priceChanged = false;

            if (product.UnitPrice != model.UnitPrice)
            {
                priceChanged = true;
            }

            string identifier = Request.GetUserEmail();

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == identifier && x.IsDeleted == false);
            if (user == null)
            {
                return BadRequest("User not found");
            }

            var basket = await _context.Baskets.FirstOrDefaultAsync(x => x.UserId == user.Id);
            if (basket == null)
            {
                // if basket is not initialized, then do this
                var newBasketItem = new BasketItem
                {
                    Quantity = model.Quantity,
                    UnitPrice = product.UnitPrice,
                    ProductId = model.ProductId
                };

                basket = new Basket();
                basket.UserId = user.Id;
                basket.BasketItems = new List<BasketItem>();
                basket.BasketItems.Add(newBasketItem);
                _context.Baskets.Add(basket);
                int rowsAffected = await _context.SaveChangesAsync();
                if (rowsAffected == 0)
                {
                    return Ok(new ResponseMessage { Success = true, Message = Messages.INSERT_ERROR, RequestRefresh = priceChanged });
                }
                //
                return Ok(new ResponseMessage { Success = true, Message = "Item added to basket", RequestRefresh = priceChanged });
            }

            var basketItem = _context.BasketItems.Where(x => x.BasketId == basket.Id)
                .Where(x => x.ProductId == model.ProductId)
                .Where(x => x.IsDeleted == false)
                .FirstOrDefault();

            if (basketItem == null)
            {
                basketItem = new BasketItem
                {
                    Quantity = model.Quantity,
                    UnitPrice = product.UnitPrice,
                    ProductId = model.ProductId
                };

                basket.BasketItems ??= new List<BasketItem>();

                basket.BasketItems.Add(basketItem);
                int rowsAffected = await _context.SaveChangesAsync();
                if (rowsAffected == 0)
                {
                    return Ok(new ResponseMessage { Success = true, Message = Messages.INSERT_ERROR, RequestRefresh = priceChanged });
                }
                //
                return Ok(new ResponseMessage { Success = true, Message = "Item added to basket", RequestRefresh = priceChanged });
            }
            else
            {
                bool isDirty = false;

                if (basketItem.Quantity != model.Quantity)
                {
                    // log here the reason
                    basketItem.Quantity = model.Quantity;
                    isDirty = true;
                }

                if (priceChanged)
                {
                    // log here the reason
                    basketItem.UnitPrice = product.UnitPrice;
                    isDirty = true;
                }

                if (isDirty)
                {
                    int rowsAffected = await _context.SaveChangesAsync();
                    if (rowsAffected == 0)
                    {
                        return Ok(new ResponseMessage { Success = true, Message = Messages.INSERT_ERROR, RequestRefresh = priceChanged });
                    }
                }

                return Ok(new ResponseMessage { Success = true, Message = "Item added to basket", RequestRefresh = priceChanged });

            }
        }

        [HttpPost("remove")]
        [Authorize(Policy = UserPermissionConsts.CAN_USE_BASKET)]
        public async Task<ActionResult> RemoveItem([FromBody] RemoveBasketItemRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var product = await _context.Products.FindAsync(model.ProductId);
            if (product == null)
            {
                // log here to check if
                return BadRequest("Product could not be found");
            }

            string identifier = Request.GetUserEmail();

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == identifier && x.IsDeleted == false);
            if (user == null)
            {
                return BadRequest("User not found");
            }

            var basket = await _context.Baskets.FirstOrDefaultAsync(x => x.UserId == user.Id);
            if (basket == null)
            {
                return Ok(new ResponseMessage { Success = true, Message = "Basket was not found for the user.", RequestRefresh = true });
            }

            var basketItem = await _context.BasketItems.Where(x => x.BasketId == basket.Id)
                .Where(x => x.ProductId == model.ProductId)
                .Where(x => x.IsDeleted == false)
                .FirstOrDefaultAsync();
            if (basketItem == null)
            {
                // returning true because item is not here
                return Ok(new ResponseMessage { Success = true, Message = "Requested item does not exist in the basket.", RequestRefresh = true });
            }

            _context.BasketItems.Remove(basketItem);
            int rowsAffected = await _context.SaveChangesAsync();
            if (rowsAffected == 0)
            {
                // because we're not sure about the ui state at that time
                return Ok(new ResponseMessage { Success = false, Message = Messages.DELETE_ERROR, RequestRefresh = true });
            }

            return Ok(new ResponseMessage { Success = true, Message = $"Item has been deleted", RequestRefresh = false });
        }

    }
}
