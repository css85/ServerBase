using System;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebTool.Identity;
using WebTool.Identity.Base;

namespace WebTool.Database
{
    public class WebToolCtx : IdentityDbContext<ApplicationUser, ApplicationRole, int>
    {
        public WebToolCtx(DbContextOptions<WebToolCtx> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>(p =>
            {
                p.ToTable("webtool_user");
            });

            modelBuilder.Entity<IdentityUserClaim<int>>(p =>
            {
                p.ToTable("webtool_user_claim");
            });

            modelBuilder.Entity<IdentityUserLogin<int>>(p =>
            {
                p.ToTable("webtool_user_login");
            });

            modelBuilder.Entity<IdentityUserToken<int>>(p =>
            {
                p.ToTable("webtool_user_token");
            });

            modelBuilder.Entity<ApplicationRole>(p =>
            {
                p.ToTable("webtool_role");

                var roleTypes = Enum.GetValues(typeof(RoleType)).Cast<RoleType>();
                foreach (var roleType in roleTypes)
                {
                    p.HasData(new ApplicationRole
                    {
                        Id = (int)roleType,
                        Name = roleType.ToString(),
                        NormalizedName = roleType.ToString().ToUpper(),
                    });
                }
            });

            modelBuilder.Entity<IdentityRoleClaim<int>>(p =>
            {
                p.ToTable("webtool_role_claim");
            });

            modelBuilder.Entity<IdentityUserRole<int>>(p =>
            {
                p.ToTable("webtool_user_role");
            });
        }
    }
}