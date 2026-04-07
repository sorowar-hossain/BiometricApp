using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiometricApp.Models
{
    public class MemberModel
    {
        public int PersonId { get; set; }
        public int UserId { get; set; }
        public int OrgId { get; set; }

        [Required(ErrorMessage = "First Name is required")]
        [MaxLength(50, ErrorMessage = "First Name cannot exceed 50 characters")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last Name is required")]
        [MaxLength(50, ErrorMessage = "Last Name cannot exceed 50 characters")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Marital Status is required")]
        public string MaritalStatus { get; set; } = string.Empty;

        public string? PlaceOfIssue { get; set; }
        public string? PlaceOfBirth { get; set; }

        [Required(ErrorMessage = "Date of Birth is required")]
        public DateTime? DateOfBirth { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "Weight cannot be negative")]
        public decimal? Weight { get; set; }

        [Required(ErrorMessage = "Father's Name is required")]
        public string FatherName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mother's Name is required")]
        public string MotherName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Expiry Date is required")]
        public DateTime? ExpiryDate { get; set; }

      
        public string PersonUniqueId { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string? UpdatedBy { get; set; }

       
    }
}
