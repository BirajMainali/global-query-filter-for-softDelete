using Microsoft.AspNetCore.Identity;

namespace SoftDeleteDemo.Entities
{
    public class User : IdentityUser<long>, ISoftDelete
    {
        
    }
}