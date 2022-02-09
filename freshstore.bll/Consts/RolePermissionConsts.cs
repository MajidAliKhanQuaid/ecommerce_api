namespace freshstore.bll.Consts
{
    public class RolePermissionConsts
    {
        // user (means user general data i.e. email/username etc.)
        public const string CAN_VIEW_USERS = "Manage.Users.View";
        public const string CAN_CREATE_USERS = "Manage.Users.Create";
        public const string CAN_UPDATE_USERS = "Manage.Users.Update";
        public const string CAN_DELETE_USERS = "Manage.Users.Delete";

        // user permissions
        public const string CAN_MANAGE_USER_PERMISSIONS = "Manage.UserPermissions";

        // can assign/remove role to user
        public const string CAN_MANAGE_USER_ROLE = "Manage.UserRole";

        // can assign/remove permission to role
        public const string CAN_MANAGE_USER_ROLES_PERMISSIONS = "Manage.UserRolePermissions";

        // order
        public const string CAN_VIEW_USER_ORDERS = "Manage.Orders.View";
        public const string CAN_UPDATE_USER_ORDERS = "Manage.Orders.Update";

        // basket
        public const string CAN_VIEW_USER_BASKETS = "Manage.Baskets.View";
        public const string CAN_UPDATE_USER_BASKETS = "Manage.Baskets.Update";

        // product
        public const string CAN_VIEW_PRODUCTS = "Manage.Products.View";
        public const string CAN_ADD_PRODUCTS = "Manage.Products.Add";
        public const string CAN_DELETE_PRODUCTS = "Manage.Products.Delete";

        // category
        public const string CAN_VIEW_CATEGORIES = "Manage.Categories.View";
        public const string CAN_ADD_CATEGORIES = "Manage.Categories.Add";
        public const string CAN_DELETE_CATEGORIES = "Manage.Categories.Delete";

    }
}
