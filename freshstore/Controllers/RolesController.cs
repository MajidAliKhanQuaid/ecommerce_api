using freshstore.bll;
using freshstore.bll.Consts;
using freshstore.bll.Dtos;
using freshstore.bll.Extensions;
using freshstore.bll.Models;
using freshstore.Requests.Role;
using freshstore.Requests.RolePermission;
using freshstore.Requests.User;
using freshstore.Responses.Address;
using freshstore.Responses.Role;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace freshstore.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    //[Route("api/v{v:apiVersion}/[controller]")]
    [Route("api/v{v:apiVersion}/roles")]
    public partial class RolesController : ControllerBase
    {
        private readonly FreshContext _context;
        private readonly ILogger<RolesController> _logger;

        public RolesController(FreshContext context, ILogger<RolesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region user roles region

        // non-admin
        [HttpGet]
        [Route("")]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(GetRoleResponse), 200)]
        [Authorize(Policy = RolePermissionConsts.CAN_MANAGE_USER_ROLE)]
        public async Task<ActionResult<List<GetRoleResponse>>> GetAllRoles()
        {
            var roles = await _context.Roles.Select(x => new GetRoleResponse
            {
                Id = x.Id,
                Name = x.Name,
                RolePermissions = x.Permissions.Select(y => new GetRolePermissionResponse { Id = y.Id, Name = y.Name }).ToList()
            }).ToListAsync();

            return Ok(roles);
        }


        // admin route (create role)
        [HttpPost]
        [Route("create")]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(CreateAddressResponse), 201)]
        [Authorize(Policy = RolePermissionConsts.CAN_MANAGE_USER_ROLE)]
        public async Task<ActionResult<CreateAddressResponse>> CreateRole([FromBody] CreateRoleRequest model)
        {
            //string identifier = Request.GetUserEmail();

            //var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == identifier && x.IsDeleted == false);
            //if (user == null)
            //{
            //    return BadRequest("User not found");
            //}

            bool roleAlreadyExists = _context.Roles.Any(x => x.Name == model.Name);
            if (roleAlreadyExists)
            {
                return BadRequest("Role already exists");
            }

            var role = new Role
            {
                Name = model.Name
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            return Created(new Uri(Request.Path, UriKind.Relative), new CreateAddressResponse { Success = true, Id = role.Id });
        }


        // non-admin
        [HttpGet]
        [Route("my")]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(GetRoleResponse), 200)]
        public async Task<ActionResult<List<GetRoleResponse>>> GetMyRoles()
        {
            string identifier = Request.GetUserEmail();

            var user = await _context.Users.Include(x => x.Roles).ThenInclude(x => x.Permissions).FirstOrDefaultAsync(x => x.Email == identifier && x.IsDeleted == false);
            if (user == null)
            {
                return BadRequest("User not found");
            }

            var roles = user.Roles.Select(x => new GetRoleResponse
            {
                Id = x.Id,
                Name = x.Name,
                RolePermissions = x.Permissions.Select(y => new GetRolePermissionResponse { Id = y.Id, Name = y.Name }).ToList()
            }).ToList();

            return Ok(roles);
        }


        // admin (get user role)
        [HttpGet]
        [Route("user/{userid}")]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(List<GetRoleResponse>), 200)]
        [Authorize(Policy = RolePermissionConsts.CAN_MANAGE_USER_ROLE)]
        public async Task<ActionResult<List<GetRoleResponse>>> GetUserRoles(long userid)
        {
            string identifier = Request.GetUserEmail();

            var user = await _context.Users.Include(x => x.Roles).ThenInclude(x => x.Permissions).FirstOrDefaultAsync(x => x.Id == userid && x.IsDeleted == false);
            if (user == null)
            {
                return BadRequest("User not found");
            }

            var roles = user.Roles.Select(x => new GetRoleResponse
            {
                Id = x.Id,
                Name = x.Name,
                RolePermissions = x.Permissions.Select(y => new GetRolePermissionResponse { Id = y.Id, Name = y.Name }).ToList()
            }).ToList();

            return Ok(roles);
        }

        // admin
        [HttpPost]
        [Route("users/{userid}/add")]
        [ProducesResponseType(typeof(Dictionary<string, string[]>), 400)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(ResponseMessage), 201)]
        [Authorize(Policy = RolePermissionConsts.CAN_MANAGE_USER_ROLE)]
        public async Task<ActionResult<ResponseMessage>> AddUserRole(long userid, [FromBody] AddUserRoleRequest model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var role = _context.Roles.Find(model.RoleId);
            if (role == null)
            {
                return NotFound("Request role does not exist");
            }

            string identifier = Request.GetUserEmail();

            var user = await _context.Users.Include(x => x.Roles).FirstOrDefaultAsync(x => x.Id == userid && x.IsDeleted == false);
            if (user == null)
            {
                return BadRequest("User not found");
            }

            var roleAlreadyExists = user.Roles.FirstOrDefault(x => x.Id == model.RoleId);
            if (roleAlreadyExists != null)
            {
                return Ok(new ResponseMessage { Success = true, Message = "Role already exists for this user" });
            }

            user.Roles.Add(role);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new ResponseMessage { Success = true, Message = "Role has been added to the user" });
        }

        // admin
        [HttpPost]
        [Route("users/{userid}/remove")]
        [ProducesResponseType(typeof(Dictionary<string, string[]>), 400)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(CreateAddressResponse), 201)]
        [Authorize(Policy = RolePermissionConsts.CAN_MANAGE_USER_ROLE)]
        public async Task<ActionResult<CreateAddressResponse>> RemoveUserRole(long userid, [FromBody] UpdateUserRoleRequest model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var role = _context.Roles.Find(model.RoleId);
            if (role == null)
            {
                return NotFound("Request role does not exist");
            }

            string identifier = Request.GetUserEmail();

            var user = await _context.Users.Include(x => x.Roles).FirstOrDefaultAsync(x => x.Email == identifier && x.IsDeleted == false);
            if (user == null)
            {
                return BadRequest("User not found");
            }

            user.Roles.Remove(role);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new ResponseMessage { Success = true, Message = "Role has been remove from the user" });
        }

       

        [HttpGet]
        [Route("rolepermissions/all")]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(CreateAddressResponse), 201)]
        [Authorize(Policy = RolePermissionConsts.CAN_MANAGE_USER_ROLES_PERMISSIONS)]
        public async Task<ActionResult<List<GetRolePermissionResponse>>> GetAllRoleLevelPermissions()
        {
            var rolePermissions = await _context.RoleLevelPermissions.Select(x => new GetRolePermissionResponse
            {
                Id = x.Id,
                Name = x.Name
            }).ToListAsync();

            return Ok(rolePermissions);
        }


        [HttpGet]
        [Route("{roleid}/permissions")]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(CreateAddressResponse), 201)]
        [Authorize(Policy = RolePermissionConsts.CAN_MANAGE_USER_ROLES_PERMISSIONS)]
        public async Task<ActionResult<List<GetRoleResponse>>> GetRoles(long roleid)
        {
            var roles = await _context.Roles.Include(x => x.Permissions)
                .Where(x => x.Id == roleid)
                .Select(x => new GetRoleResponse
                {
                    Name = x.Name,
                    RolePermissions = x.Permissions.Select(x => new GetRolePermissionResponse { Id = x.Id, Name = x.Name }).ToList()
                })
                .FirstOrDefaultAsync();

            return Ok(roles);
        }


        [HttpPost]
        [Route("permissions/add")]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(CreateAddressResponse), 200)]
        [Authorize(Policy = RolePermissionConsts.CAN_MANAGE_USER_ROLES_PERMISSIONS)]
        public async Task<ActionResult<ResponseMessage>> AddRolePermissions([FromBody] UpdateRolePermissionRequest model)
        {
            var permission = _context.RoleLevelPermissions.Find(model.RolePermissionId);
            if (permission == null)
            {
                return BadRequest("Requeted permission does not exist");
            }

            var role = await _context.Roles.Include(x => x.Permissions)
                .Where(x => x.Id == model.RoleId)
                .FirstOrDefaultAsync();

            if (role == null)
            {
                return BadRequest("Requested role does not exist");
            }

            if (role.Permissions.Any(x => x.Id == model.RolePermissionId))
            {
                return Ok(new ResponseMessage { Success = true, Message = "Role permission already exists" });
            }

            role.Permissions.Add(permission);
            _context.Roles.Update(role);
            await _context.SaveChangesAsync();

            return Ok(new ResponseMessage { Success = true, Message = "Role permission has been added to role" });
        }


        [HttpPost]
        [Route("permissions/remove")]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(CreateAddressResponse), 200)]
        [Authorize(Policy = RolePermissionConsts.CAN_MANAGE_USER_ROLES_PERMISSIONS)]
        public async Task<ActionResult<ResponseMessage>> RemoveRolePermissions([FromBody] UpdateRolePermissionRequest model)
        {
            var permission = _context.RoleLevelPermissions.Find(model.RolePermissionId);
            if (permission == null)
            {
                return BadRequest("Requeted permission does not exist");
            }

            var role = await _context.Roles.Include(x => x.Permissions)
                .Where(x => x.Id == model.RoleId)
                .FirstOrDefaultAsync();

            if (role == null)
            {
                return BadRequest("Requested role does not exist");
            }

            if (!role.Permissions.Any(x => x.Id == model.RolePermissionId))
            {
                return Ok(new ResponseMessage { Success = true, Message = "Role permission does not exist for the role" });
            }

            role.Permissions.Remove(permission);
            _context.Roles.Update(role);
            await _context.SaveChangesAsync();

            return Ok(new ResponseMessage { Success = true, Message = "Role permission has been remove to role" });
        }


        #endregion
    }
}
