namespace SmartServices.models.Entity
{
    public class CandidateShortlisted
    {
        public int JobId { get; set; }          // Matches "JobId"
        public int CandidateId { get; set; }   // Matches "CandidateId"
        public string first_name { get; set; } // Matches "first_name"
        public string? middle_name { get; set; }// Matches "middle_name"
        public string last_name { get; set; }  // Matches "last_name"
        public string latestrole { get; set; } // Matches "latestrole"
        public string education { get; set; }  // Matches "education"
        public string Star { get; set; }       // Matches "Star"
        public string experience { get; set; } // Matches "experience"
        public string skills { get; set; }     // Matches "skills"
        public string LatestStatus { get; set; }// Matches "LatestStatus"
        public string email { get; set; }      // Matches "email"

        public string J_GUID { get; set; }
    }
}
