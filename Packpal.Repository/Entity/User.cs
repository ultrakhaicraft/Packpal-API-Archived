using System.ComponentModel.DataAnnotations;
using Packpal.DAL.Enum;

namespace Packpal.DAL.Entity
{
    public class User
    {
		[Key]
		public Guid Id { get; set; } = Guid.NewGuid();
        [Required(ErrorMessage = "Email is required")]
		public string Email { get; set; } = string.Empty;
		[Required(ErrorMessage = "Password is required")]
		public string Password { get; set; } = string.Empty;
		[Required(ErrorMessage = "Username is required")]
		public string Username { get; set; } = string.Empty;
		[Required(ErrorMessage = "Phone Number is required")]
		public string PhoneNumber { get; set; } = string.Empty;
		public List<string>? Roles { get; set; } = new List<string> { RoleEnum.RENTER.ToString() }; // default role is RENTER
		public string? ActiveRole { get; set; } = RoleEnum.RENTER.ToString(); // current active role
        public string Status { get; set; } = UserStatusEnum.ACTIVE.ToString(); // default status is ACTIVE
		public string? AvatarUrl { get; set; }

        // Navigation property
        public virtual Keeper? Keeper { get; set; }
        public virtual Renter? Renter { get; set; }
    }
}