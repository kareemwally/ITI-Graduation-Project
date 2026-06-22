using DAL.Models.Common;

namespace DAL.Models
{
    /// <summary>
    /// Role lookup. Modelled as a table (not an enum) so new admin roles can be added without a migration.
    /// </summary>
    public class Role : BaseEntity
    {
        public string RoleName { get; set; } = null!;

        // Navigation
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
