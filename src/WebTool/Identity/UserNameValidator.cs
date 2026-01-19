using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace WebTool.Identity
{
    public class UserNameValidator<TUser> : IUserValidator<TUser>
        where TUser : IdentityUser
    {
        public Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user)
        {
            foreach (var c in user.UserName)
            {
                if ((c < '0' || c > '9') &&
                    (c < 'a' || c > 'Z') &&
                    (c < '가' || c > '힣'))
                {
                    return Task.FromResult(IdentityResult.Failed(new IdentityError
                    {
                        Code = "InvalidCharactersUserName",
                        Description = "이름은 숫자, 알파벳, 한글만 가능합니다."
                    }));
                }
            }

            return Task.FromResult(IdentityResult.Success);
        }
    }
}
