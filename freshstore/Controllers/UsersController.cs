using freshstore.bll;
using freshstore.bll.Consts;
using freshstore.bll.Dtos;
using freshstore.bll.Enums;
using freshstore.bll.Extensions;
using freshstore.bll.Models;
using freshstore.Requests.Address;
using freshstore.Requests.Role;
using freshstore.Requests.RolePermission;
using freshstore.Requests.User;
using freshstore.Requests.UserPermission;
using freshstore.Responses.Address;
using freshstore.Responses.Role;
using freshstore.Responses.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

namespace freshstore.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    //[Route("api/v{v:apiVersion}/[controller]")]
    [Route("api/v{v:apiVersion}/users")]
    public partial class UsersController : ControllerBase
    {
        private readonly FreshContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(FreshContext context, ILogger<UsersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        // this is an admin route
        [Authorize(Policy = RolePermissionConsts.CAN_VIEW_USERS)]
        public async Task<ActionResult<IEnumerable<GetUserReponse>>> Get()
        {
            var users = await _context.Users.Select(x => new GetUserReponse
            {
                Id = x.Id,
                Email = x.Email,
                Name = x.Name,
            }).ToListAsync();

            return Ok(users);
        }

        [HttpGet]
        [Route("{id}")]
        // this is an admin route
        [Authorize(Policy = RolePermissionConsts.CAN_VIEW_USERS)]
        public async Task<ActionResult<User>> Get(int id)
        {
            var user = await _context.Users
                .Select(x => new GetUserReponse
                {
                    Id = x.Id,
                    Email = x.Email,
                    Name = x.Name,
                }).FirstOrDefaultAsync(x => x.Id == id);
            return Ok(user);
        }

        [HttpGet]
        [Route("basket/{id}")]
        // this is an admin route
        [Authorize(Policy = RolePermissionConsts.CAN_VIEW_USER_BASKETS)]
        public async Task<ActionResult<IEnumerable<BasketModel>>> Basket(int id)
        {
            var basket = await _context.Baskets
                                .Where(x => x.UserId == id)
                                .Include(x => x.BasketItems)
                                .Select(x => new BasketModel
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
                                .FirstOrDefaultAsync();
            return Ok(basket);
        }

        // only normal users can be registered here
        [AllowAnonymous]
        [HttpPost]
        [Route("register")]
        [ProducesResponseType(typeof(Dictionary<string, string[]>), 400)]
        [ProducesResponseType(typeof(ResponseMessage), 201)]
        public async Task<ActionResult<User>> Register([FromBody] RegisterUserRequest model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            string ipAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "N/A";
            var dupeEmail = _context.Users.Any(x => x.Email == model.Email);
            if (dupeEmail)
            {
                ModelState.AddModelError(nameof(freshstore.bll.Models.User.Email), "Email already exists.");
                return BadRequest(ModelState);
            }

            var userLevelPermissions = await _context.UserLevelPermissions.Where(x => x.IsDeleted == false).ToListAsync();

            var user = new User { Email = model.Email, Name = model.Name, Password = model.Password, LastAccessedIp = ipAddress, Permissions = userLevelPermissions, IsConfirmed = true };
            var newUser = await _context.Users.AddAsync(user);
            int result = await _context.SaveChangesAsync();
            if (result == 0) return BadRequest(Messages.INSERT_ERROR);
            return Created(new Uri($"{Request.Path}", UriKind.Relative), new ResponseMessage { Success = true, Message = "User registered successfully !", Data = new { Id = user.Id, Name = user.Name, Email = user.Email } });
        }

        // only normal users can be registered here
        [HttpPost]
        [Route("registernonuser")]
        [ProducesResponseType(typeof(Dictionary<string, string[]>), 400)]
        [ProducesResponseType(typeof(ResponseMessage), 201)]
        [Authorize(Policy = RolePermissionConsts.CAN_CREATE_USERS)]
        public async Task<ActionResult<User>> RegisterNonUser([FromBody] RegisterNonUserRequest model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            string ipAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "N/A";
            var dupeEmail = _context.Users.Any(x => x.Email == model.Email);
            if (dupeEmail)
            {
                ModelState.AddModelError(nameof(RegisterNonUserRequest.Email), "Email already exists.");
                return BadRequest(ModelState);
            }

            if (model.Roles?.Count() == 0)
            {
                ModelState.AddModelError(nameof(freshstore.bll.Models.User.Roles), "Roles must be defined.");
                return BadRequest(ModelState);
            }

            model.Roles = model.Roles.Distinct().ToArray();
            var roles = await _context.Roles.Where(x => model.Roles.Contains(x.Name) && x.IsDeleted == false).ToListAsync();
            if (roles.Count != model.Roles.Count())
            {
                var invalidRoles = roles.Where(x => !model.Roles.Contains(x.Name)).ToList();
                ModelState.AddModelError(nameof(RegisterNonUserRequest.Roles), $"Enter valid roles. {string.Join(",", invalidRoles)} not found.");
                return BadRequest(ModelState);
            }

            var user = new User { Email = model.Email, Name = model.Name, Password = model.Password, LastAccessedIp = ipAddress, IsConfirmed = false, Roles = roles };
            var newUser = await _context.Users.AddAsync(user);
            int result = await _context.SaveChangesAsync();
            if (result == 0) return BadRequest(Messages.INSERT_ERROR);
            return Created(new Uri($"{Request.Path}", UriKind.Relative), new ResponseMessage { Success = true, Message = "User registered successfully !", Data = new { Id = user.Id, Name = user.Name, Email = user.Email } });
        }

        //[HttpPost]
        //public async Task<ActionResult<User>> Post([FromBody] User user)
        //{
        //    var newUser = await _context.Users.AddAsync(user);
        //    int result = await _context.SaveChangesAsync();
        //    if (result == 0) return BadRequest(Messages.INSERT_ERROR);
        //    return Created(new Uri(Request.Path, UriKind.Relative), newUser.Entity);
        //}

        //[HttpPut("{id}")]
        //public async Task<ActionResult<User>> Put(int id, [FromBody] User user)
        //{
        //    var usrToUpdate = await _context.Users.FindAsync(id);
        //    if (usrToUpdate == null) return BadRequest(Messages.NOT_FOUND_MESSAGE);
        //    usrToUpdate.Name = user.Name;
        //    int result = await _context.SaveChangesAsync();
        //    if (result == 0) return BadRequest(Messages.UPDATE_ERROR);
        //    return usrToUpdate;
        //}

        [HttpDelete("{id}")]
        [Authorize(Policy = RolePermissionConsts.CAN_DELETE_USERS)]
        public async Task<ActionResult> Delete(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound(Messages.NOT_FOUND_MESSAGE);
            _context.Users.Remove(user);
            int result = await _context.SaveChangesAsync();
            if (result == 0) return BadRequest(Messages.DELETE_ERROR);
            return Ok(new { deleted = 1 });
        }

        #region address section

        [HttpPost]
        [Route("address/create")]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(CreateAddressResponse), 201)]
        public async Task<ActionResult<CreateAddressResponse>> CreateAddress([FromBody] CreateAddressRequest model)
        {
            string identifier = Request.GetUserEmail();

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == identifier && x.IsDeleted == false);
            if (user == null)
            {
                return BadRequest("User not found");
            }

            var address = new Address
            {
                Address1 = model.Address1,
                Address2 = model.Address2,
                Phone = model.Phone,
                Country = model.Country,
                City = model.City,
                State = model.State,
                ZipCode = model.ZipCode,
                Type = model.Type,
                UserId = user.Id
            };

            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();

            return Created(new Uri(Request.Path, UriKind.Relative), new CreateAddressResponse { Success = true, Id = address.Id });
        }

        [HttpPut]
        [Route("address/update")]
        [ProducesResponseType(typeof(Dictionary<string, string[]>), 400)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(UpdateAddressResponse), 200)]
        public async Task<ActionResult<UpdateAddressResponse>> UpdateAddress([FromBody] UpdateAddressRequest model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            string identifier = Request.GetUserEmail();

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == identifier && x.IsDeleted == false);
            if (user == null)
            {
                return BadRequest("User not found");
            }

            var address = await _context.Addresses.FirstOrDefaultAsync(x => x.Id == model.Id && x.UserId == user.Id && x.IsDeleted == false);
            if (address == null)
            {
                return NotFound("Address was not found");
            }

            // adding comparison for logging user activity
            if (!string.Equals(address.Address1, model.Address1, StringComparison.OrdinalIgnoreCase))
            {
                address.Address1 = model.Address1;
            }

            if (!string.Equals(address.Address2, model.Address2, StringComparison.OrdinalIgnoreCase))
            {
                address.Address2 = model.Address2;
            }

            if (!string.Equals(address.Phone, model.Phone, StringComparison.OrdinalIgnoreCase))
            {
                address.Phone = model.Phone;
            }

            if (!string.Equals(address.Country, model.Country, StringComparison.OrdinalIgnoreCase))
            {
                address.Country = model.Country;
            }

            if (!string.Equals(address.City, model.City, StringComparison.OrdinalIgnoreCase))
            {
                address.City = model.City;
            }

            if (!string.Equals(address.State, model.State, StringComparison.OrdinalIgnoreCase))
            {
                address.State = model.State;
            }

            if (!string.Equals(address.ZipCode, model.ZipCode, StringComparison.OrdinalIgnoreCase))
            {
                address.ZipCode = model.ZipCode;
            }

            if (address.Type != model.Type)
            {
                address.Type = model.Type;
            }

            _context.Addresses.Update(address);
            await _context.SaveChangesAsync();

            return Ok(new UpdateAddressResponse { Success = true });
        }

        [HttpGet]
        [Route("address")]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(GetAddressResponse), 200)]
        public async Task<ActionResult<List<GetAddressResponse>>> Addresses([FromQuery] AddressType? type)
        {
            string identifier = Request.GetUserEmail();

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == identifier && x.IsDeleted == false);
            if (user == null)
            {
                return BadRequest("User not found");
            }

            var addressQuery = _context.Addresses.Where(x => x.UserId == user.Id && x.IsDeleted == false);
            if (type.HasValue)
            {
                addressQuery = addressQuery.Where(x => x.Type == type.Value);
            }

            var addresses = await addressQuery.Select(a => new GetAddressResponse
            {
                Id = a.Id,
                Address1 = a.Address1,
                Address2 = a.Address2,
                Phone = a.Phone,
                Country = a.Country,
                City = a.City,
                State = a.State,
                ZipCode = a.ZipCode,
                Type = a.Type
            }).ToListAsync();

            return Ok(addresses);
        }

        #endregion

        #region user level permissions (used by non-user i.e. admins)

        [HttpGet]
        [Route("permissions/{userid}")]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(GetAddressResponse), 200)]
        public async Task<ActionResult<List<string>>> GetUserPermission(long userid)
        {
            string identifier = Request.GetUserEmail();

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == identifier && x.IsDeleted == false);
            if (user == null)
            {
                return Unauthorized("User not found");
            }

            var reqUser = await _context.Users.Include(x => x.Permissions).FirstOrDefaultAsync(x => x.Id == userid);
            if (reqUser == null)
            {
                return BadRequest("User for which permission were to be updated, does not exist");
            }

            List<string> permissions = reqUser.Permissions.Select(x => x.Name).ToList();

            return Ok(permissions);
        }

        [HttpPost]
        [Route("permissions/{userid}/grantone")]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(ResponseMessage), 200)]
        public async Task<ActionResult<ResponseMessage>> UpdateUserPermission(long userid, [FromBody] UpdateUserPermissionRequest model)
        {
            string identifier = Request.GetUserEmail();

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == identifier && x.IsDeleted == false);
            if (user == null)
            {
                return Unauthorized("User not found");
            }

            var permission = await _context.UserLevelPermissions.FirstOrDefaultAsync(x => x.Name == model.Permission && x.IsDeleted == false);
            if (permission == null)
            {
                return BadRequest("The requested permission does not exist");
            }

            var reqUser = await _context.Users.Include(x => x.Permissions).FirstOrDefaultAsync(x => x.Id == userid);
            if (reqUser == null)
            {
                return BadRequest("User for which permission were to be updated, does not exist");
            }

            bool alreadyPermitted = reqUser.Permissions.Any(x => x.Name == model.Permission);
            if (alreadyPermitted)
            {
                return Ok(new ResponseMessage { Success = true, Message = "Permissions was already granted" });
            }

            reqUser.Permissions.Add(permission);
            _context.Users.Update(reqUser);

            await _context.SaveChangesAsync();

            return Ok(new ResponseMessage { Success = true, Message = "Permissions have been updated" });
        }

        [HttpPost]
        [Route("permissions/{userid}/grant")]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(ResponseMessage), 200)]
        public async Task<ActionResult<ResponseMessage>> UpdateMultipleUserPermissions(long userid, [FromBody] List<UpdateUserPermissionRequest> model)
        {
            string identifier = Request.GetUserEmail();

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == identifier && x.IsDeleted == false);
            if (user == null)
            {
                return Unauthorized("User not found");
            }

            // removing any duplication from requested permissions
            List<string> reqPermissions = model.DistinctBy(x => x.Permission).Select(x => x.Permission).ToList();

            var permissions = await _context.UserLevelPermissions.ToListAsync();

            // validating permission names
            var invalidPermission = permissions.Where(x => !reqPermissions.Contains(x.Name)).ToList();
            if (invalidPermission.Count > 0)
            {
                return BadRequest($"Permissions  {string.Join(", ", invalidPermission.Select(x => x.Name))} does not exist");
            }

            var reqUser = await _context.Users.Include(x => x.Permissions).FirstOrDefaultAsync(x => x.Id == userid);
            if (reqUser == null)
            {
                return BadRequest("User for which permission were to be updated, does not exist");
            }

            List<string> permissionAlreadyGranted = new List<string>();
            List<string> newPermissions = new List<string>();

            foreach (var permissionName in reqPermissions)
            {
                var permission = permissions.FirstOrDefault(x => x.Name == permissionName);
                if (permission == null) continue; // this shouldn't come but added check

                bool alreadyGranted = reqUser.Permissions.Any(x => x.Name.Equals(permissionName, StringComparison.OrdinalIgnoreCase));
                if (alreadyGranted)
                {
                    permissionAlreadyGranted.Add(permissionName);
                }
                else
                {
                    reqUser.Permissions.Add(permission);
                    newPermissions.Add(permissionName);
                }
            }

            string message = string.Empty;
            if (permissionAlreadyGranted.Count > 0)
            {
                message += $"{string.Join(", ", permissionAlreadyGranted)} were already granted. {Environment.NewLine}";
            }

            if (newPermissions.Count > 0)
            {
                message += $"{string.Join(", ", newPermissions)} are granted now";
            }

            _context.Users.Update(reqUser);
            await _context.SaveChangesAsync();

            return Ok(new ResponseMessage { Success = true, Message = message });
        }

        [HttpPost]
        [Route("permissions/{userid}/removeone")]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(ResponseMessage), 200)]
        public async Task<ActionResult<ResponseMessage>> RemoveUserPermission(long userid, [FromBody] UpdateUserPermissionRequest model)
        {
            string identifier = Request.GetUserEmail();

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == identifier && x.IsDeleted == false);
            if (user == null)
            {
                return Unauthorized("User not found");
            }

            var permission = await _context.UserLevelPermissions.FirstOrDefaultAsync(x => x.Name == model.Permission && x.IsDeleted == false);
            if (permission == null)
            {
                return BadRequest("The requested permission does not exist");
            }

            var reqUser = await _context.Users.Include(x => x.Permissions).FirstOrDefaultAsync(x => x.Id == userid);
            if (reqUser == null)
            {
                return BadRequest("User for which permission were to be updated, does not exist");
            }

            if (!reqUser.Permissions.Any(x => x.Name == permission.Name))
            {
                return Ok(new ResponseMessage { Success = true, Message = "Permissions not granted to user" });
            }

            reqUser.Permissions.Remove(permission);
            _context.Users.Update(reqUser);

            await _context.SaveChangesAsync();

            return Ok(new ResponseMessage { Success = true, Message = "Permissions have been removed" });
        }

        [HttpPost]
        [Route("permissions/{userid}/remove")]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(ResponseMessage), 200)]
        public async Task<ActionResult<ResponseMessage>> RemoveMultipleUserPermissions(long userid, [FromBody] List<UpdateUserPermissionRequest> model)
        {
            string identifier = Request.GetUserEmail();

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == identifier && x.IsDeleted == false);
            if (user == null)
            {
                return Unauthorized("User not found");
            }

            // removing any duplication from requested permissions
            List<string> reqPermissions = model.DistinctBy(x => x.Permission).Select(x => x.Permission).ToList();

            var permissions = await _context.UserLevelPermissions.ToListAsync();
            var permissionNames = permissions.Select(x => x.Name).ToList();
            // validating permission names
            var invalidPermission = reqPermissions.Where(x => !permissionNames.Contains(x)).ToList(); // permissions.Where(x => !reqPermissions.Contains(x.Name)).ToList();
            //var invalidPermission = permissions.Where(x => !reqPermissions.Contains(x.Name)).ToList();
            if (invalidPermission.Count > 0)
            {
                return BadRequest($"Invalid permissions {string.Join(", ", invalidPermission.Select(x => $"`{x}`"))}.");
            }

            var reqUser = await _context.Users.Include(x => x.Permissions).FirstOrDefaultAsync(x => x.Id == userid);
            if (reqUser == null)
            {
                return BadRequest("User for which permission were to be updated, does not exist");
            }

            List<string> toBeRemovedPermissions = new List<string>();
            List<string> notExistsPermissions = new List<string>();

            foreach (var permissionName in reqPermissions)
            {
                var permission = permissions.FirstOrDefault(x => x.Name == permissionName);
                if (permission == null) continue; // this shouldn't come but added check

                bool toBeRemoved = reqUser.Permissions.Any(x => x.Name.Equals(permissionName, StringComparison.OrdinalIgnoreCase));
                if (toBeRemoved)
                {
                    reqUser.Permissions.Remove(permission);
                    toBeRemovedPermissions.Add(permissionName);
                }
                else
                {
                    notExistsPermissions.Add(permissionName);
                }
            }

            string message = string.Empty;
            if (toBeRemovedPermissions.Count > 0)
            {
                message += $"{string.Join(", ", toBeRemovedPermissions)} were removed. {Environment.NewLine}";
            }

            if (notExistsPermissions.Count > 0)
            {
                message += $"{string.Join(", ", notExistsPermissions)} did not exist for the user.";
            }

            _context.Users.Update(reqUser);
            await _context.SaveChangesAsync();

            return Ok(new ResponseMessage { Success = true, Message = message });
        }

        #endregion


    }
}
