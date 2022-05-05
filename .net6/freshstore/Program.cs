using freshstore.bll;
using Serilog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog.Sinks.MSSqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Authorization;
using freshstore.Authorization;
using Microsoft.OpenApi.Models;
using freshstore.Config;
using freshstore.Security;
using Microsoft.IdentityModel.Tokens;
using freshstore.Authorization.Handlers;
using freshstore.Authorization.Requirements;
using System.Configuration;
using freshstore.Authorization.Requirements.Order;
using freshstore.Authorization.Requirements.Product;
using freshstore.Authorization.Requirements.Category;
using freshstore.bll.Consts;
using freshstore.Authorization.Handlers.Category;
using freshstore.Authorization.Handlers.Order;
using freshstore.Authorization.Handlers.Product;

var builder = WebApplication.CreateBuilder(args);
string corsPolicy = builder.Configuration.GetValue<string>("CorsPolicy");
string connectionString = builder.Configuration.GetConnectionString("Default");
//string signSecret = builder.Configuration.GetValue<string>("JwtSettings:SigningSecret");
//string encSecret = builder.Configuration.GetValue<string>("JwtSettings:EncryptionSecret");
//string issuer = builder.Configuration.GetValue<string>("JwtSettings:JwtIssuer");

// registering handlers for authorization policies
//builder.Services.AddSingleton<IAuthorizationHandler, MinimumAgeHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, CanViewCategoriesHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, CanViewOrdersHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, CanViewProductsHandler>();


builder.Services.AddTransient<TokenManagementService>();

/*
 * 
 * Adding config to concrete classes (In controller access it by injecting IOptions<T>
 * 
 */

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection("JwtSettings"));

// ----------------- method # 1
//var jwtSettings = new JwtOptions();
//builder.Configuration.GetSection("JwtSettings").Bind(jwtSettings);

// ----------------- method # 2
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtOptions>();

/*
 * 
 * Using Serilog for Logging
 * 
 */

builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console()
    .WriteTo.MSSqlServer(
        connectionString: connectionString,
        sinkOptions: new MSSqlServerSinkOptions { AutoCreateSqlTable = true, TableName = "LogEvents" },
        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Verbose));
// .WriteTo.Seq("http://localhost:5341"));


/*
 * 
 * Using Memory Cache for data caching in API
 * 
 */

builder.Services.AddMemoryCache();

/*
 * 
 * Using CORS policy to enable Cross Origin Requests to API
 * 
 */

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: corsPolicy,
        builder =>
        {
            builder.AllowAnyOrigin();
            builder.AllowAnyHeader();
            //builder.WithHeaders(HeaderNames.ContentType);
            //builder.with("PUT", "POST", "GET");
            builder.AllowAnyMethod();
        });
});

/*
 * 
 * Adding Jwt Authentication for API Endpoints
 * 
 */

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SigningSecret)),
            TokenDecryptionKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.EncryptionSecret)),
        };
    });

builder.Services.AddAuthorization(options =>
{
    //options.AddPolicy("Atleast21", policy => policy.Requirements.Add(new MinimumAgeRequirement(21)));

    /*
     * 
     * 
     * 
     * 
     * admin
     */

    // admin - user management
    options.AddPolicy(RolePermissionConsts.CAN_CREATE_USERS, policy => policy.RequireClaim(RolePermissionConsts.CAN_CREATE_USERS));
    options.AddPolicy(RolePermissionConsts.CAN_VIEW_USERS, policy => policy.RequireClaim(RolePermissionConsts.CAN_VIEW_USERS));
    options.AddPolicy(RolePermissionConsts.CAN_DELETE_USERS, policy => policy.RequireClaim(RolePermissionConsts.CAN_DELETE_USERS));

    // admin - roles  management
    options.AddPolicy(RolePermissionConsts.CAN_MANAGE_USER_ROLE, policy => policy.RequireClaim(RolePermissionConsts.CAN_MANAGE_USER_ROLE));

    // admin - roles permission management
    options.AddPolicy(RolePermissionConsts.CAN_MANAGE_USER_ROLES_PERMISSIONS, policy => policy.RequireClaim(RolePermissionConsts.CAN_MANAGE_USER_ROLES_PERMISSIONS));

    // admin - baskets management
    options.AddPolicy(RolePermissionConsts.CAN_VIEW_USER_BASKETS, policy => policy.RequireClaim(RolePermissionConsts.CAN_VIEW_USER_BASKETS));
    
    // admin - products management
    options.AddPolicy(RolePermissionConsts.CAN_ADD_PRODUCTS, policy => policy.RequireClaim(RolePermissionConsts.CAN_ADD_PRODUCTS));
    
    // admin - categories management
    options.AddPolicy(RolePermissionConsts.CAN_ADD_CATEGORIES, policy => policy.RequireClaim(RolePermissionConsts.CAN_ADD_CATEGORIES));

    /*
     * 
     * 
     * 
     * users
     */

    // basket
    options.AddPolicy(UserPermissionConsts.CAN_USE_BASKET, policy => policy.RequireClaim(UserPermissionConsts.CAN_USE_BASKET));

    // product
    options.AddPolicy(UserPermissionConsts.CAN_VIEW_PRODUCTS, policy => policy.RequireClaim(UserPermissionConsts.CAN_USE_BASKET));

    // order
    options.AddPolicy(UserPermissionConsts.CAN_VIEW_ORDERS, policy => policy.RequireClaim(UserPermissionConsts.CAN_USE_BASKET));
    options.AddPolicy(UserPermissionConsts.CAN_PLACE_ORDER, policy => policy.RequireClaim(UserPermissionConsts.CAN_USE_BASKET));

    /*
     * 
     * 
     * 
     * commmon - both admin - users
     */

    options.AddPolicy(CommonPermissionConsts.CAN_VIEW_ORDERS, policy => policy.Requirements.Add(new CanViewOrdersRequirement()));
    options.AddPolicy(CommonPermissionConsts.CAN_VIEW_PRODUCTS, policy => policy.Requirements.Add(new CanViewProductsRequirement()));
    options.AddPolicy(CommonPermissionConsts.CAN_VIEW_CATEGORIES, policy => policy.Requirements.Add(new CanViewCategoriesRequirement()));

});

/*
 * 
 * Adding API Versioning to API
 * 
 */

builder.Services.AddApiVersioning(x =>
{
    x.DefaultApiVersion = new ApiVersion(1, 0);
    x.AssumeDefaultVersionWhenUnspecified = true;
    x.ReportApiVersions = true;
});


builder.Services.AddVersionedApiExplorer(setup =>
{
    setup.GroupNameFormat = "'v'VVV";
    //setup.GroupNameFormat = "VVVV";
    setup.SubstituteApiVersionInUrl = true;
});

/*
 * 
 * Adding DB Context for Ef Core (using lazy loading)
 * 
 */

builder.Services.AddDbContext<FreshContext>(options => options
        .UseLazyLoadingProxies()
        .UseSqlServer(connectionString));
//

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();

//builder.Services.AddVersionedApiExplorer(options =>
// {
//     options.GroupNameFormat = "VVV";
//     options.SubstituteApiVersionInUrl = true;
// });



//builder.Services.AddSwaggerGen(c => {
//    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Fresh Store API", Version = "v1" });
//    var securityScheme =
//        new OpenApiSecurityScheme
//        {
//            In = ParameterLocation.Header,
//            Description = "Please enter into field the word 'Bearer' following by space and JWT",
//            Name = "Authorization",
//            Type = SecuritySchemeType.OAuth2
//        };
//    c.AddSecurityDefinition("Bearer", securityScheme);

//    var securityRequirement = new OpenApiSecurityRequirement();
//    securityRequirement.Add(securityScheme, new List<string> { "Bearer" });
//    c.AddSecurityRequirement(securityRequirement);

//});
//


builder.Services.AddSwaggerGen(options =>
{
    //options.SwaggerDoc("1.0", new OpenApiInfo
    //{
    //    Title = "Fresh Store API",
    //    Version = "1.0",
    //    Description = "Buys Fresh Items at your door step",
    //    Contact = new OpenApiContact
    //    {
    //        Name = "majid ali khan quaid",
    //        Url = new Uri("https://majidalikhanquaid.github.io")
    //    },
    //    License = new OpenApiLicense()
    //    {
    //        Name = "Commercial",
    //        Url = new Uri("https://majidalikhanquaid.github.io")
    //    }
    //});

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                    }
                },
                new string[] {}
        }
    });
});


//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new OpenApiInfo
//    {
//        Title = "Fresh Store API",
//        Version = "v1",
//        Description = "Buys Fresh Items at your door step",
//        Contact = new OpenApiContact
//        {
//            Name = "majid ali khan quaid",
//            Url = new Uri("https://majidalikhanquaid.github.io")
//        },
//        License = new OpenApiLicense()
//        {
//            Name = "Commercial",
//            Url = new Uri("https://majidalikhanquaid.github.io")
//        }
//    });

//    var securityScheme =
//        new OpenApiSecurityScheme
//        {
//            In = ParameterLocation.Header,
//            Name = HeaderNames.Authorization,
//            Description = "Please enter into field the word 'Bearer' following by space and JWT",
//            BearerFormat = "JWT",
//            Scheme = JwtBearerDefaults.AuthenticationScheme,
//            Type = SecuritySchemeType.Http,
//        };

//    //c.AddSecurityDefinition("Bearer", securityScheme);
//    c.AddSecurityDefinition("jwt_auth", securityScheme);

//    var securityRequirement = new OpenApiSecurityRequirement();
//    securityRequirement.Add(securityScheme, new List<string> { });
//    c.AddSecurityRequirement(securityRequirement);

//});


/*
 * 
 * Using HttpAccessor for access http context in custom classes (especially in Context class)
 * 
 */

builder.Services.AddHttpContextAccessor();

//builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
//builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

builder.Services.AddControllers();

//----------------------------------------------------------------------------------
//----------------------------------------------------------------------------------
//----------------------------------------------------------------------------------



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Insta Cart");
        // to enable it to run on '/' route rather than '/swagger/index.html'
        c.RoutePrefix = String.Empty;
    });
}

/*
 * 
 * Using CORS middlware using policy specified above)
 * 
 */

app.UseCors(corsPolicy);

/*
 * 
 * Https Redirection to auto redirect to https if not specified
 * 
 */

app.UseHttpsRedirection();

/*
 * 
 * Using authentication middleware (must be before authorization)
 * you can skip this line, if you want to explicitly define authentication scheme on each endpoint
* i.e.
* [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
* 
 */

app.UseAuthentication();

/*
 * 
 * Using authorization middleware (must be after authentication)
 * 
 */

app.UseAuthorization();

/*
 * 
 * Using endpoints for API Controllers; 
 * Enabling Authorization on all endpoints by default
 * 
 */

app.MapControllers().RequireAuthorization();

/*
 * 
 * Running the app
 * 
 */

app.Run();
