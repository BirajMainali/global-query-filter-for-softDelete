using Microsoft.AspNetCore.Identity;

namespace SoftDeleteDemo.Entities
{
    public interface IRecordInfo
    {
        public IdentityUser? User { get; set; }
    }
}