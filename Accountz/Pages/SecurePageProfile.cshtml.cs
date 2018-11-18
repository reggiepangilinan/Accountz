using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Accountz.Pages
{
    [Authorize]
    public class SecurePageProfileModel : PageModel
    {
        public void OnGet()
        {
            
        }
    }
}