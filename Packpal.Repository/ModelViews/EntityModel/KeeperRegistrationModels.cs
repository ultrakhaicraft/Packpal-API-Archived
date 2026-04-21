using System.ComponentModel.DataAnnotations;

namespace Packpal.DAL.ModelViews.EntityModel
{
    /// <summary>
    /// Data model for keeper registration stored in Request.Data field
    /// </summary>
    public class KeeperRegistrationData
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string IdentityNumber { get; set; }

        [Required]
        public required string BankAccount { get; set; }

        [Required]
        public required string DocumentsUrl { get; set; } // URL to uploaded PDF document
    }

    /// <summary>
    /// Model for creating keeper registration request via mobile app
    /// </summary>
    public class CreateKeeperRegistrationRequestModel
    {
        public Guid UserId { get; set; }
        
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string IdentityNumber { get; set; }

        [Required]
        public required string BankAccount { get; set; }

        [Required]
        public required string DocumentsUrl { get; set; } // URL to uploaded PDF document
    }

    /// <summary>
    /// Response model for keeper registration request
    /// </summary>
    public class KeeperRegistrationRequestResponse
    {
        public Guid RequestId { get; set; }
        public string Status { get; set; } = "PENDING";
        public string Message { get; set; } = "Your keeper registration request has been submitted and is pending approval.";
        public DateTime RequestedAt { get; set; }
    }

    /// <summary>
    /// Model for staff to register keeper from approved request
    /// </summary>
    public class RegisterKeeperFromRequestModel
    {
        [Required]
        public Guid UserId { get; set; }
        
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string IdentityNumber { get; set; }

        [Required]
        public required string BankAccount { get; set; }

        [Required]
        public required string DocumentsUrl { get; set; }
    }
}
