namespace SmartServices.models.Dto
{
    public class JobSaveResponseDto
    {
        public string id { get; set; }
        public string jobTitle { get; set; }
        public Dictionary<string, string> jobLocation { get; set; }
        public string jobType { get; set; }
        public List<string> jobResponsibilities { get; set; }
        public Dictionary<string, string> jobQualifications { get; set; }
        public string jobRoleOverview { get; set; }
        public string jobPostingDate { get; set; }
        public string jobDescriptionText { get; set; }
        public string jobHiringManager { get; set; }
    }

}
