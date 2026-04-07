namespace BiometricApi.Entities
{
    public class Biometric
    {
        public int PersonId { get; set; }
        public int UserId { get; set; }
        public int OrgId { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MaritalStatus { get; set; }
        public string? PlaceOfIssue { get; set; }
        public string? PlaceOfBirth { get; set; }

        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public decimal? Weight { get; set; }

        public string FatherName { get; set; }
        public string MotherName { get; set; }

        public DateTime ExpiryDate { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public string PersonUniqueId { get; set; }
    }
}
