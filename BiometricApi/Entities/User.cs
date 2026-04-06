using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BiometricApi.Entities
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }           // Primary Key

        public int OrgId { get; set; }
        public int RoleId { get; set; }

        [Required]
        public string UserName { get; set; } = null!;

        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        [Required]
        public string Password { get; set; } = null!;   // Store hashed password

        public bool IsActive { get; set; }

        public DateTime CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
