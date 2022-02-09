using freshstore.bll;
using freshstore.bll.Consts;
using freshstore.bll.Dtos;
using freshstore.bll.Models;
using freshstore.Requests.Product;
using freshstore.Responses.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace freshstore.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{v:apiVersion}/products")]
    public partial class ProductController : ControllerBase
    {
        private readonly FreshContext _context;
        private readonly ILogger<ProductController> _logger;

        public ProductController(FreshContext context, ILogger<ProductController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Policy = CommonPermissionConsts.CAN_VIEW_PRODUCTS)]
        public async Task<ActionResult<IEnumerable<GetProductResponse>>> Get()
        {
            var product = await _context.Products
                .Include(x => x.Categories)
                .Select(x => new GetProductResponse
                {
                    Id = x.Id,
                    Name = x.Name,
                    UnitPrice = x.UnitPrice,
                    Categories = x.Categories.Select(x => new CategoryModel { CategoryName = x.Name }).ToList()
                })
                .ToListAsync();

            return Ok(product);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = CommonPermissionConsts.CAN_VIEW_PRODUCTS)]
        public async Task<ActionResult<GetProductResponse>> Get(int id)
        {
            var product = await _context.Products
                .Include(x => x.Categories)
                .Select(x => new GetProductResponse
                {
                    Id = x.Id,
                    Name = x.Name,
                    UnitPrice = x.UnitPrice,
                    Categories = x.Categories.Select(x => new CategoryModel { CategoryName = x.Name }).ToList()
                })
                .FirstOrDefaultAsync(x => x.Id == id);

            return Ok(product);
        }

        [HttpPost]
        [Route("create")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Dictionary<string, string[]>))]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CreateProductResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Policy = RolePermissionConsts.CAN_ADD_PRODUCTS)]
        public async Task<ActionResult> Post([FromBody] CreateProductRequest model)
        {
            try
            {
                bool exits = await _context.Categories.AnyAsync(x => x.Name == model.Name);
                if (exits)
                {
                    ModelState.AddModelError(nameof(CreateProductResponse.Name), $"Product name `{model.Name}` already exits");
                    return BadRequest(ModelState);
                }

                var product = new Product { Name = model.Name, UnitPrice = model.UnitPrice };
                product.Categories ??= new List<Category>();

                foreach (var ProductCategory in model.Categories ?? new List<CategoryModel>())
                {
                    var category = _context.Categories.Find(ProductCategory.CategoryId);
                    if (category == null) continue;
                    product.Categories.Add(category);
                }
                _context.Products.Add(product);

                await _context.SaveChangesAsync();

                var resProduct = new CreateProductResponse
                {
                    Id = product.Id,
                    Name = product.Name,
                    UnitPrice = product.UnitPrice,
                    Quantity = int.MaxValue,
                    Categories = product.Categories.Select(x => new CategoryModel { CategoryName = x.Name }).ToList()
                };

                return Created(new Uri($"{Request.Path}", UriKind.Relative), resProduct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

    }
}
