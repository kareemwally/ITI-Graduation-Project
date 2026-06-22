namespace DAL.Models
{
    /// <summary>
    /// Many-to-many join between Users and Roles. Composite primary key (UserId, RoleId).
    /// </summary>
    public class UserRole
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }

        // Navigation
        public User User { get; set; } = null!;
        public Role Role { get; set; } = null!;
    }
}
