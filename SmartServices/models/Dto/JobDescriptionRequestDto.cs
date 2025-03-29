namespace SmartServices.models.Dto
{
    public class JobDescriptionRequestDto
    {
        public string jobTitle { get; set; }
        public string jobExperienceRequired { get; set; }
        public string jobLocation { get; set; }
        public List<string> jobPrimarySkills { get; set; }
        public List<string> jobSecondarySkills { get; set; }
        public List<string> jobEducationalQualifications { get; set; }
        public string jobBusinessDependencies { get; set; }
        public string jobRole { get; set; }
        public string jobType { get; set; }
    }
}
