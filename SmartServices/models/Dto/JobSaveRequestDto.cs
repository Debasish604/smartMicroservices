namespace SmartServices.models.Dto
{
    public class JobSaveRequestDto
    {
        public string jobTitle { get; set; }
        public string jobExperienceRequired { get; set; }
        public string jobLocation { get; set; }
        public List<string> jobPrimarySkills { get; set; }
        public List<string> jobSecondarySkills { get; set; }
        public List<string> jobEducationalQualifications { get; set; }
        public string jobRole { get; set; }
        public string jobType { get; set; }
        public string jobHiringManager { get; set; }
        public Dictionary<string, object> jobDescriptionText { get; set; }
    }

}
