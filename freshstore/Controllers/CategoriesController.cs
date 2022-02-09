using freshstore.bll;
using freshstore.bll.Consts;
using freshstore.bll.Dtos;
using freshstore.bll.Helpers;
using freshstore.bll.Models;
using freshstore.Requests.Category;
using freshstore.Responses.Category;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace freshstore.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{v:apiVersion}/categories")]
    public partial class CategoriesController : ControllerBase
    {
        private readonly IMemoryCache _memoryCache;
        private readonly FreshContext _context;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(IMemoryCache memoryCache, FreshContext context, ILogger<CategoriesController> logger)
        {
            _memoryCache = memoryCache;
            _context = context;
            _logger = logger;
        }

        // common route
        [HttpGet]
        [Authorize(Policy = CommonPermissionConsts.CAN_VIEW_CATEGORIES)]
        public async Task<ActionResult<IEnumerable<GetCategoryResponse>>> Get()
        {
            var canViewCategoriesClaim = HttpContext.User.FindFirst(
                c => (c.Type == RolePermissionConsts.CAN_VIEW_CATEGORIES || c.Type == UserPermissionConsts.CAN_VIEW_CATEGORIES) && c.Value == "1");

            #region method 1

            //var categories = await _memoryCache.GetOrCreateAsync<IEnumerable<CategoryModel>>(CacheKeys.CATEGORIES, async factory =>
            //{
            //    factory.SetSize(1);
            //    factory.SetSlidingExpiration(TimeSpan.FromMinutes(10));
            //    var categories = await _context.Categories.Select(x => new CategoryModel
            //    {
            //        CategoryName = x.Name
            //    }).ToListAsync();
            //    return categories;
            //});

            #endregion

            #region method 2


            bool isCached = _memoryCache.TryGetValue<IEnumerable<GetCategoryResponse>>(CacheKeys.CATEGORIES, out var categories);
            if (isCached)
            {
                CacheLogger.LogCache(Messages.LOG_HIT, CacheKeys.CATEGORIES, categories);
                return Ok(categories);
            }

            CacheLogger.LogCache(Messages.LOG_MISS, CacheKeys.CATEGORIES, null);

            categories = await _context.Categories.Select(x => new GetCategoryResponse
            {
                CategoryName = x.Name
            })
            .ToListAsync();

            var cacheEntryOptions = new MemoryCacheEntryOptions();
            cacheEntryOptions.SetSize(1);
            cacheEntryOptions.SetSlidingExpiration(TimeSpan.FromMinutes(10));
            _memoryCache.Set(CacheKeys.CATEGORIES, categories, cacheEntryOptions);

            #endregion

            return Ok(categories);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CategoryModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [Authorize(Policy = CommonPermissionConsts.CAN_VIEW_CATEGORIES)]
        public async Task<ActionResult<GetCategoryResponse>> Get(int id)
        {
            bool isCached = _memoryCache.TryGetValue<IEnumerable<CategoryModel>>(CacheKeys.CATEGORIES, out var categories);
            if (isCached)
            {
                CacheLogger.LogCache(Messages.LOG_HIT, CacheKeys.CATEGORIES, categories);
                //
                var cachedCategory = categories.FirstOrDefault(x => x.CategoryId == id);
                if (cachedCategory != null)
                {
                    return Ok(cachedCategory);
                }
            }

            // if not found in cache, then query the database
            var category = await _context.Categories.FirstOrDefaultAsync(x => x.Id == id);
            if (category == null)
            {
                return NotFound("Category not found");
            }

            var categoryModel = new GetCategoryResponse
            {
                CategoryId = category.Id,
                CategoryName = category.Name
            };

            return Ok(categoryModel);
        }

        // admin route
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Dictionary<string, string[]>))]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CategoryModel))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Policy = RolePermissionConsts.CAN_ADD_CATEGORIES)]
        public async Task<ActionResult> Post([FromBody] CreateCategoryRequest model)
        {
            try
            {
                bool exits = await _context.Categories.AnyAsync(x => x.Name == model.CategoryName);
                if (exits)
                {
                    ModelState.AddModelError(nameof(CategoryModel.CategoryName), $"category name `{model.CategoryName}` already exits");
                    return BadRequest(ModelState);
                }

                var category = new Category { Name = model.CategoryName };
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                // incase of adding a new category, we'll empty the cache
                bool isCached = _memoryCache.TryGetValue<IEnumerable<CategoryModel>>(CacheKeys.CATEGORIES, out var categories);
                if (isCached)
                {
                    _memoryCache.Remove(CacheKeys.CATEGORIES);
                }

                var resCategory = new GetCategoryResponse { CategoryName = category.Name };

                return Created(new Uri($"{Request.Path}", UriKind.Relative), resCategory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

    }
}
