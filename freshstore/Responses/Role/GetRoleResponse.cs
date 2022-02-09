namespace freshstore.Responses.Role
{
    public class GetRoleResponse
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public List<GetRolePermissionResponse> RolePermissions { get; set; }
    }
}
