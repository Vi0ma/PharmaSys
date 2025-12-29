using Microsoft.AspNetCore.Identity;

namespace PharmaSys.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? RoleLabel { get; set; } // ex: Pharmacien
    }
}
