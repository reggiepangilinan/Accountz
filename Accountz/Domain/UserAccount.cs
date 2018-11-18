using Microsoft.AspNetCore.Identity;

namespace Accountz.Domain
{
    public class UserAccount : IdentityUser
    {
        public string Firstname { get; set; }
        public string Middlename { get; set; }
        public string Lastname { get; set; }
        public string Photo { get; set; }
    }
}
