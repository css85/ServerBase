namespace WebTool.Identity
{
    public enum RoleType
    {
        User = 1,
        Gm = 2,
        Admin = 3,
        Developer = 4,
    }

    public static class AuthorizeRoleType
    {
        public const string AuthorizeUser = nameof(RoleType.User) + "," + nameof(RoleType.Gm) + "," + nameof(RoleType.Admin) + "," + nameof(RoleType.Developer);
        public const string AuthorizeGm = nameof(RoleType.Gm) + "," + nameof(RoleType.Admin) + "," + nameof(RoleType.Developer);
        public const string AuthorizeAdmin = nameof(RoleType.Admin) + "," + nameof(RoleType.Developer);
        public const string AuthorizeDeveloper = nameof(RoleType.Developer);
    }
}