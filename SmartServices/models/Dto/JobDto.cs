namespace SmartServices.models.Dto
{
    public class JobDto
    {
        public int Id { get; set; }
        public string JD { get; set; }
        public string Experience { get; set; }
        public string Location { get; set; }
        public string Role { get; set; }
        public string PrimarySkills { get; set; }
        public string SecondarySkills { get; set; }
        public string BusinessDependencies { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string HiringManagerId { get; set; }
    }
}

