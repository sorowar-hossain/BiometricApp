using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BiometricApi.Entities
{
    public class Organization
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrgId { get; set; }             // Primary Key

        [Required]
        public string OrganizationName { get; set; } = null!;  // Maps "Organization" column

        public bool IsActive { get; set; }

        [Required]
        public string Code { get; set; } = null!;  // Unique code like "D-001"

        public string? Address { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
