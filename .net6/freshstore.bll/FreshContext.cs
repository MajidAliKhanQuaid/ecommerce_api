using freshstore.bll.Consts;
using freshstore.bll.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Security.Claims;

namespace freshstore.bll
{
    public class FreshContext : DbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FreshContext(IHttpContextAccessor httpContextAccessor, DbContextOptions<FreshContext> options) : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<UserLevelPermission> UserLevelPermissions { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<RoleLevelPermission> RoleLevelPermissions { get; set; }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Basket> Baskets { get; set; }
        public DbSet<BasketItem> BasketItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<CacheLog> CacheLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<Order>().Property<string>(x => x.UpdatedBy).HasDefaultValue(null);
            //modelBuilder.Entity<Order>().Property<string>(x => x.UpdatedBy).HasMaxLength(100);

            //foreach(var entity in modelBuilder.Model.GetEntityTypes())
            //{
            //    var property = entity.GetProperties().FirstOrDefault(p => p.Name == "UpdatedBy");
            //    if(property != null)
            //    {
            //        property.SetDefaultValue("null");
            //    }
            //}

            modelBuilder.Entity<User>()
                .HasIndex(x => x.Email)
                .IsUnique();

            modelBuilder.Entity<User>().HasMany(x => x.Baskets)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId);

            modelBuilder.Entity<User>()
                .HasMany(x => x.Addresses)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId);

            modelBuilder.Entity<User>()
                .HasMany(x => x.Orders)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId);

            modelBuilder.Entity<User>()
                .HasMany(x => x.Roles)
                .WithMany(x => x.Users)
                .UsingEntity("UserRoles");

            modelBuilder.Entity<User>()
                .HasMany(x => x.Permissions)
                .WithMany(x => x.Users)
                .UsingEntity("UserPermissions");

            modelBuilder.Entity<Role>()
                .HasMany(x => x.Permissions)
                .WithMany(x => x.Roles)
                .UsingEntity("RolePermissions");

            modelBuilder.Entity<Role>()
                .HasIndex(x => x.Name)
                .IsUnique();

            modelBuilder.Entity<RoleLevelPermission>()
                .HasIndex(x => x.Name)
                .IsUnique();

            modelBuilder.Entity<UserLevelPermission>()
                .HasIndex(x => x.Name)
                .IsUnique();

            modelBuilder.Entity<Product>()
                .HasMany(x => x.Categories)
                .WithMany(x => x.Products)
                .UsingEntity("ProductCategories");

            modelBuilder.Entity<Order>()
                .HasMany(x => x.OrderItems)
                .WithOne(x => x.Order)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Basket>()
                .HasMany(x => x.BasketItems)
                .WithOne(x => x.Basket)
                .HasForeignKey(x => x.BasketId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BasketItem>()
                .HasKey(x => new { x.BasketId, x.ProductId });

            modelBuilder.Entity<BasketItem>().HasCheckConstraint(nameof(BasketItem.Quantity), $"[{nameof(BasketItem.Quantity)}] > 0");

            modelBuilder.Entity<OrderItem>().HasCheckConstraint(nameof(OrderItem.Quantity), $"[{nameof(OrderItem.Quantity)}] > 0");

            //modelBuilder.Entity<Order>().Property(x => x.UpdatedBy);

            #region Seeding Data

            //
            // ************* adding user permissions
            //

            //string[] userPermissions = { "UP.AddToCart", "UP.RemoveFromCart", "UP.PlaceAnOrder", "UP.CancelAnOrder", "UP.ChangeEmail", "UP.ChangeMobile" };

            //
            // ************* adding role permissions 
            //

            var rolePermissions = new List<RoleLevelPermission>();
            // users
            var p_CAN_VIEW_USERS = new RoleLevelPermission { Id = 1, Name = RolePermissionConsts.CAN_VIEW_USERS };
            var p_CAN_CREATE_USERS = new RoleLevelPermission { Id = 2, Name = RolePermissionConsts.CAN_CREATE_USERS };
            var p_CAN_UPDATE_USERS = new RoleLevelPermission { Id = 3, Name = RolePermissionConsts.CAN_UPDATE_USERS };
            var p_CAN_DELETE_USERS = new RoleLevelPermission { Id = 4, Name = RolePermissionConsts.CAN_DELETE_USERS };
            // role
            var p_CAN_MANAGE_USER_ROLE = new RoleLevelPermission { Id = 5, Name = RolePermissionConsts.CAN_MANAGE_USER_ROLE };
            // permissions
            var p_CAN_MANAGE_USER_PERMISSIONS = new RoleLevelPermission { Id = 6, Name = RolePermissionConsts.CAN_MANAGE_USER_PERMISSIONS };
            // orders
            var p_CAN_VIEW_USER_ORDERS = new RoleLevelPermission { Id = 7, Name = RolePermissionConsts.CAN_VIEW_USER_ORDERS };
            var p_CAN_UPDATE_USER_ORDERS = new RoleLevelPermission { Id = 8, Name = RolePermissionConsts.CAN_UPDATE_USER_ORDERS };
            // baskets
            var p_CAN_VIEW_USER_BASKETS = new RoleLevelPermission { Id = 9, Name = RolePermissionConsts.CAN_VIEW_USER_BASKETS };
            var p_CAN_UPDATE_USER_BASKETS = new RoleLevelPermission { Id = 10, Name = RolePermissionConsts.CAN_UPDATE_USER_BASKETS };
            // products
            var p_CAN_VIEW_PRODUCTS = new RoleLevelPermission { Id = 11, Name = RolePermissionConsts.CAN_VIEW_PRODUCTS };
            var p_CAN_ADD_PRODUCTS = new RoleLevelPermission { Id = 12, Name = RolePermissionConsts.CAN_ADD_PRODUCTS };
            var p_CAN_DELETE_PRODUCTS = new RoleLevelPermission { Id = 13, Name = RolePermissionConsts.CAN_DELETE_PRODUCTS };
            // categories
            var p_CAN_VIEW_CATEGORIES = new RoleLevelPermission { Id = 14, Name = RolePermissionConsts.CAN_VIEW_CATEGORIES };
            var p_CAN_ADD_CATEGORIES = new RoleLevelPermission { Id = 15, Name = RolePermissionConsts.CAN_ADD_CATEGORIES };
            var p_CAN_DELETE_CATEGORIES = new RoleLevelPermission { Id = 16, Name = RolePermissionConsts.CAN_DELETE_CATEGORIES };
            //
            rolePermissions.Add(p_CAN_VIEW_USERS);
            rolePermissions.Add(p_CAN_UPDATE_USERS);
            rolePermissions.Add(p_CAN_DELETE_USERS);
            //
            rolePermissions.Add(p_CAN_MANAGE_USER_ROLE);
            //
            rolePermissions.Add(p_CAN_MANAGE_USER_PERMISSIONS);
            //
            rolePermissions.Add(p_CAN_VIEW_USER_ORDERS);
            rolePermissions.Add(p_CAN_UPDATE_USER_ORDERS);
            //
            rolePermissions.Add(p_CAN_VIEW_USER_BASKETS);
            rolePermissions.Add(p_CAN_UPDATE_USER_BASKETS);
            //
            rolePermissions.Add(p_CAN_VIEW_PRODUCTS);
            rolePermissions.Add(p_CAN_ADD_PRODUCTS);
            rolePermissions.Add(p_CAN_DELETE_PRODUCTS);
            //
            rolePermissions.Add(p_CAN_VIEW_CATEGORIES);
            rolePermissions.Add(p_CAN_ADD_CATEGORIES);
            rolePermissions.Add(p_CAN_DELETE_CATEGORIES);
            //
            for (int i = 0; i < rolePermissions.Count; i++)
            {
                modelBuilder.Entity<RoleLevelPermission>().HasData(new RoleLevelPermission
                {
                    Id = rolePermissions[i].Id,
                    Name = rolePermissions[i].Name
                });
            }

            //
            // ************* adding roles
            //

            var userRoles = new List<Role>();
            //
            var superAdminRole = new Role { Id = 1, Name = RoleConsts.SUPER_ADMIN };
            var orderMgrRole = new Role { Id = 2, Name = RoleConsts.ORDER_MANAGER };
            var catalogMgrRole = new Role { Id = 3, Name = RoleConsts.CATALOG_MANAGER };
            //
            userRoles.Add(superAdminRole);
            userRoles.Add(orderMgrRole);
            userRoles.Add(catalogMgrRole);
            //
            for (int i = 0; i < userRoles.Count; i++)
            {
                modelBuilder.Entity<Role>().HasData(new Role
                {
                    Id = userRoles[i].Id,
                    Name = userRoles[i].Name
                });
            }

            var userPermissions = new List<UserLevelPermission>();
            //
            var up_CAN_LOG = new UserLevelPermission { Id = 1, Name = UserPermissionConsts.CAN_LOG };
            var up_CAN_USE_BASKET = new UserLevelPermission { Id = 2, Name = UserPermissionConsts.CAN_USE_BASKET };
            var up_CAN_VIEW_PRODUCTS = new UserLevelPermission { Id = 3, Name = UserPermissionConsts.CAN_VIEW_PRODUCTS };
            var up_CAN_VIEW_CATEGORIES = new UserLevelPermission { Id = 4, Name = UserPermissionConsts.CAN_VIEW_CATEGORIES };
            var up_CAN_VIEW_ORDERS = new UserLevelPermission { Id = 5, Name = UserPermissionConsts.CAN_VIEW_ORDERS };
            var up_CAN_UPDATE_ORDER = new UserLevelPermission { Id = 6, Name = UserPermissionConsts.CAN_UPDATE_ORDER };
            var up_CAN_PLACE_ORDER = new UserLevelPermission { Id = 7, Name = UserPermissionConsts.CAN_PLACE_ORDER };
            //
            userPermissions.Add(up_CAN_LOG);
            userPermissions.Add(up_CAN_USE_BASKET);
            userPermissions.Add(up_CAN_VIEW_PRODUCTS);
            userPermissions.Add(up_CAN_VIEW_CATEGORIES);
            userPermissions.Add(up_CAN_VIEW_ORDERS);
            userPermissions.Add(up_CAN_UPDATE_ORDER);
            userPermissions.Add(up_CAN_PLACE_ORDER);
            //
            for (int i = 0; i < userPermissions.Count; i++)
            {
                modelBuilder.Entity<UserLevelPermission>().HasData(new UserLevelPermission
                {
                    Id = userPermissions[i].Id,
                    Name = userPermissions[i].Name
                });
            }

            //
            // ************* adding users
            //

            var users = new List<User>();
            //
            User user1 = new User { Id = 1, Name = "Majid Ali Khan Quaid", Email = "contactmakq@gmail.com", Password = "12345" };
            User user2 = new User { Id = 2, Name = "John Doe", Email = "a@b.com", Password = "12345" };
            User adminUser = new User { Id = 3, Name = "Super Admin", Email = "admin@store.com", Password = "12345" };
            //
            users.Add(user1);
            users.Add(user2);
            users.Add(adminUser);
            //
            for (int i = 0; i < users.Count(); i++)
            {
                var user = users.ElementAt(i);
                modelBuilder.Entity<User>().HasData(new User
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Password = user.Password,
                    CreatedBy = "Seeder",
                    LastAccessedIp = "::1",
                });
            }

            //
            // ************* adding categories
            //

            var categories = new List<Category>();
            var freshCategory = new Category { Id = 1, Name = "Fresh" };
            var organicCategory = new Category { Id = 2, Name = "Organic" };
            var fruitCategory = new Category { Id = 3, Name = "Fruit" };
            var vegCategory = new Category { Id = 4, Name = "Vegetable" };
            var electronicCategory = new Category { Id = 5, Name = "Electronics" };
            var mobileCategory = new Category { Id = 5, Name = "Mobile" };
            var laptopCategory = new Category { Id = 5, Name = "Laptop" };
            //
            categories.Add(freshCategory);
            categories.Add(organicCategory);
            categories.Add(fruitCategory);
            categories.Add(vegCategory);
            categories.Add(electronicCategory);
            categories.Add(mobileCategory);
            categories.Add(laptopCategory);

            //
            // ************* adding products
            //

            Dictionary<string, Category[]> products = new Dictionary<string, Category[]>();
            products.Add("Kiwi", new Category[] { freshCategory, organicCategory, fruitCategory });
            products.Add("Potato", new Category[] { freshCategory, organicCategory, vegCategory });
            products.Add("Milk", new Category[] { freshCategory, organicCategory });
            products.Add("Iphone 13", new Category[] { electronicCategory, mobileCategory });
            products.Add("HP Bs 085nia", new Category[] { electronicCategory, laptopCategory });
            //
            for (int i = 0; i < products.Count(); i++)
            {
                var product = products.ElementAt(i);
                modelBuilder.Entity<Product>().HasData(new Product
                {
                    Id = i + 1,
                    Name = product.Key,
                    UnitPrice = (i + 1) * 10,
                    //Categories = product.Value.ToList(),
                    CreatedBy = "Seeder"
                });
            }
            //modelBuilder.Entity<Product>()
            //    .HasMany(x => x.Categories)
            //    .WithMany(x => x.Products)
            //    .UsingEntity(e => 
            //        e.ToTable("ProductCategories")
            //        .HasData(new[]
            //        {
            //            new { ProductId = 1, CategoriesId = 1},
            //            new { ProductId = 1, CategoriesId = 2},
            //            new { ProductId = 1, CategoriesId = 3},

            //            new { ProductId = 2, CategoriesId = 1},
            //            new { ProductId = 2, CategoriesId = 2},
            //            new { ProductId = 2, CategoriesId = 4},
            //        })
            //    );


            //modelBuilder
            //    .Entity<Product>()
            //    .UsingEntity(j => j
            //    .ToTable("UserTechnology")
            //    .HasData(new[]
            //        {
            //            { UsersID = 1, TechnologiesID = 1 },
            //            { UsersID = 1, TechnologiesID = 2 }
            //        }
            //    ));

            #endregion

            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges()
        {
            UpdateDefaultFields();
            return base.SaveChanges();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            UpdateDefaultFields();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            UpdateDefaultFields();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateDefaultFields();
            return base.SaveChangesAsync(cancellationToken);
        }

        // updating created/updated by/on fields

        private void UpdateDefaultFields()
        {
            string loggedInuser = "System";
            if (_httpContextAccessor.HttpContext != null)
            {
                loggedInuser = _httpContextAccessor.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value ?? "System";
            }

            // setting up user tracking info
            foreach (var e in this.ChangeTracker
                .Entries()
                .ToList())
            {
                PropertyInfo property;
                PropertyInfo[] properties = e.Entity.GetType().GetProperties();
                if (e.State == EntityState.Added)
                {
                    #region populating created date field
                    property = properties.FirstOrDefault(p => p.Name.EndsWith("CreatedOn"));
                    if (property != null)
                    {
                        property.SetValue(e.Entity, DateTime.UtcNow);
                    }
                    #endregion

                    #region populating created by field
                    property = properties.FirstOrDefault(p => p.Name.EndsWith("CreatedBy"));
                    if (property != null && property.GetValue(e.Entity) == null)
                    {
                        property.SetValue(e.Entity, loggedInuser);
                    }
                    #endregion
                }
                if (e.State == EntityState.Modified)
                {
                    #region populating modified date field
                    property = properties.FirstOrDefault(p => p.Name.EndsWith("UpdatedOn"));
                    if (property != null)
                    {
                        property.SetValue(e.Entity, DateTime.UtcNow);
                    }
                    #endregion

                    #region populating modified by field
                    //property = properties.FirstOrDefault(p => p.Name.EndsWith("UpdatedBy"));

                    //if (property != null)
                    //{
                    //    var propName = property.GetValue(e.Entity);
                    //    if (_isTaskBased)
                    //    {
                    //        //DO NOT update this filed in case of task based.
                    //        // if (propName == null)
                    //        //    property.SetValue(e.Entity, loggedInuser);
                    //        _isTaskBased = false;
                    //    }
                    //    else
                    //        property.SetValue(e.Entity, loggedInuser);
                    //}
                    #endregion

                    #region populating changed by field
                    property = properties.FirstOrDefault(p => p.Name.EndsWith("UpdatedBy"));
                    if (property != null)
                    {
                        property.SetValue(e.Entity, loggedInuser);
                    }
                    #endregion
                    // here excluding Created properties from being updated (as those values will be set as null as not available in the current context)

                    #region excluding Created date from the update 
                    property = properties.FirstOrDefault(p => p.Name.EndsWith("created_date"));
                    if (property != null)
                    {
                        e.Property(property.Name).IsModified = false;
                    }
                    #endregion

                    #region excluding Created by from the update 
                    property = properties.FirstOrDefault(p => p.Name.EndsWith("created_by"));
                    if (property != null)
                    {
                        e.Property(property.Name).IsModified = false;
                    }
                    #endregion
                }

            }
        }


    }
}