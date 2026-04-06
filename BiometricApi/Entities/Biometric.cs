namespace BiometricApi.Entities
{
    public class Biometric
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        public byte[] FaceImage { get; set; }
        public byte[] Fingerprint { get; set; }
        public byte[] Iris { get; set; }

        //public User User { get; set; }
    }
}
