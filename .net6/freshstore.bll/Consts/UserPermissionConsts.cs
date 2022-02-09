namespace freshstore.bll.Consts
{
    public class UserPermissionConsts
    {
        // can login
        public const string CAN_LOG = "User.Login";

        // can use basket
        public const string CAN_USE_BASKET = "User.Basket";

        // can view products
        public const string CAN_VIEW_PRODUCTS = "User.Products.View";

        // can view categories
        public const string CAN_VIEW_CATEGORIES = "User.Categories.View";

        // can use order management
        public const string CAN_VIEW_ORDERS = "User.Orders.View";
        public const string CAN_UPDATE_ORDER = "User.Orders.Update";
        public const string CAN_PLACE_ORDER = "User.Orders.Place";
    }
}
