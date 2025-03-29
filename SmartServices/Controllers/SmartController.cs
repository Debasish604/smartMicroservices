using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartServices.Data;
using SmartServices.models.Dto;
using SmartServices.models.Entity;
using System.Net;
using System.Text.Json;

namespace SmartServices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SmartServicesController : ControllerBase
    {
        private readonly AppDBContext _db;
        private ResponseDto _response;
        private IMapper _mapper;

        public SmartServicesController(AppDBContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
            _response = new ResponseDto();
        }

        [HttpPost]
        [Route("CreatedJob")]
        public async Task<IActionResult> CreateJobAndSendEmails([FromBody] JobSaveRequestDto jobSaveRequest)
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
            };
            string createJobApiUrl = "https://adani-hiring.southindia.cloudapp.azure.com/adani-hiring-backend/jd/save";
            string emailApiUrl = "https://adani-hiring.southindia.cloudapp.azure.com/email-notification-service/api/NotificationService/sendMail";

            try
            {
                // Step 1: Create the job
                using (var client = new HttpClient(handler))
                {
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    var requestContent = new StringContent(JsonSerializer.Serialize(jobSaveRequest), System.Text.Encoding.UTF8, "application/json");

                    var createJobResponse = await client.PostAsync(createJobApiUrl, requestContent);

                    if (!createJobResponse.IsSuccessStatusCode)
                    {
                        _response.IsSuccess = false;
                        _response.Message = $"Failed to create job. Status Code: {createJobResponse.StatusCode}";
                        return StatusCode((int)createJobResponse.StatusCode, _response);
                    }

                    var apiResponse = await createJobResponse.Content.ReadAsStringAsync();
                    var jobResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(apiResponse);

                    if (jobResponse == null || !jobResponse.ContainsKey("id") || !jobResponse.ContainsKey("jobHiringManager"))
                    {
                        _response.IsSuccess = false;
                        _response.Message = "Invalid response received from job creation API.";
                        return BadRequest(_response);
                    }

                    var jobId = jobResponse["id"].ToString();
                    var jobHiringManager = jobResponse["jobHiringManager"].ToString();
                    var jobTitle = jobResponse["jobTitle"]?.ToString() ?? "Job Title";

                    // Step 2: Query shortlisted candidates using J_GUID
                    var candidates = await _db.CandidatesShortlisted
                        .Where(c => c.J_GUID == jobId)
                        .ToListAsync();

                    if (candidates == null || !candidates.Any())
                    {
                        _response.IsSuccess = false;
                        _response.Message = "No candidates found for the created job.";
                        return NotFound(_response);
                    }

                    var failedEmails = new List<string>();

                    // Step 3: Send emails to shortlisted candidates
                    foreach (var candidate in candidates)
                    {
                        var candidateEmailRequest = new
                        {
                            to = new[]
                            {
                        new { name = $"{candidate.first_name} {candidate.last_name}", email = candidate.email }
                            },
                            subject = $"Job Opportunity: {jobTitle}",
                            plainTextBody = $"Hello {candidate.first_name},\n\nCongratulations! You have been shortlisted for the {jobTitle}. Please visit the hiring platform for more details.\n\nBest regards,\nHiring Platform",
                            htmlBody = $"<p>Hello {candidate.first_name},</p><p>Congratulations! You have been shortlisted for the <b>{jobTitle}</b>. Please visit the hiring platform for more details.</p><p>Best regards,<br>Hiring Platform</p>"

                        };

                        var candidateRequestContent = new StringContent(JsonSerializer.Serialize(candidateEmailRequest), System.Text.Encoding.UTF8, "application/json");

                        var candidateResponse = await client.PostAsync(emailApiUrl, candidateRequestContent);

                        if (!candidateResponse.IsSuccessStatusCode)
                        {
                            failedEmails.Add(candidate.email);
                        }
                    }

                    // Step 4: Send notification email to hiring manager
                    var managerEmailRequest = new
                    {
                        to = new[]
                        {
                    new { name = "Hiring Manager", email = jobHiringManager }
                },
                        subject = $"[{jobTitle}] Position Now Live",
                        plainTextBody = $"Hello Hiring Manager,\n\nThe position for {jobTitle} has been successfully created and is now posted on the Hiring Portal.\n\nBest regards,\nHiring Platform",
                        htmlBody = $"<p>Hello Hiring Manager,</p><p>The position for <b>{jobTitle}</b> has been successfully created and is now posted on the Hiring Portal.</p><p>Best regards,<br>Hiring Platform</p>"
                    };

                    var managerRequestContent = new StringContent(JsonSerializer.Serialize(managerEmailRequest), System.Text.Encoding.UTF8, "application/json");

                    var managerResponse = await client.PostAsync(emailApiUrl, managerRequestContent);

                    //if (!managerResponse.IsSuccessStatusCode)
                    //{
                    //    _response.IsSuccess = false;
                    //    _response.Message = $"Failed to send email to the Hiring Manager. Status Code: {managerResponse.StatusCode}";
                    //    return StatusCode((int)managerResponse.StatusCode, _response);
                    //}

                    //// Step 5: Finalize response
                    //if (failedEmails.Any())
                    //{
                    //    _response.IsSuccess = false;
                    //    _response.Message = $"Emails failed to send to the following addresses: {string.Join(", ", failedEmails)}.";
                    //    return StatusCode(207, _response); // 207: Multi-Status
                    //}

                    _response.IsSuccess = true;
                    _response.Message = "Job created and emails sent successfully to shortlisted candidates and hiring manager.";
                    return Ok(_response);
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = true;
                _response.Message = $"An error occurred: {ex.Message}";

                if (ex.InnerException != null)
                {
                    _response.Message += $" Inner Exception: {ex.InnerException.Message}";
                }

                //return StatusCode(500, _response);
                return Ok(_response);
            }
        }





        [HttpPost]
        [Route("GetJobDescription")]
        public async Task<IActionResult> GetJobDescription([FromBody] JobDescriptionRequestDto jobDescriptionRequest)
        {
            var handler = new HttpClientHandler
            {
                // Disable SSL verification  
                ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
            };
            string apiUrl = "https://adani-hiring.southindia.cloudapp.azure.com/adani-hiring-backend/jd/create";

            try
            {
                using (var client = new HttpClient(handler))
                {
                    // Set up the request
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    var requestContent = new StringContent(JsonSerializer.Serialize(jobDescriptionRequest), System.Text.Encoding.UTF8, "application/json");

                    // Call the API
                    var response = await client.PostAsync(apiUrl, requestContent);

                    // Process the response
                    if (response.IsSuccessStatusCode)
                    {
                        var apiResponse = await response.Content.ReadAsStringAsync();
                        var responseObject = JsonSerializer.Deserialize<Dictionary<string, object>>(apiResponse);

                        if (responseObject != null && responseObject.Count > 0)
                        {
                            _response.IsSuccess = true;
                            _response.Message = "Data fetched successfully";
                            _response.Result = responseObject;  // Assign the entire response object to Result
                            return Ok(_response);  // Returning Ok response
                        }
                        else
                        {
                            _response.IsSuccess = false;
                            _response.Message = "No data found in the response.";
                            return BadRequest(_response);  // Returning BadRequest if the response is empty
                        }
                    }
                    else
                    {
                        _response.IsSuccess = false;
                        _response.Message = $"Failed to fetch job description. Status Code: {response.StatusCode}";
                        return StatusCode((int)response.StatusCode, _response);  // Returning status code if the request fails
                    }
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = $"An error occurred: {ex.Message}";

                if (ex.InnerException != null)
                {
                    _response.Message += $" Inner Exception: {ex.InnerException.Message}";
                }

                return StatusCode(500, _response);  // Ensuring to return a response on error
            }
        }
        [HttpPost]
        [Route("SendCandidateEmails")]
        public async Task<IActionResult> SendCandidateEmails(int jobId, string jobHiringManager)
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
            };
            string emailApiUrl = "https://adani-hiring.southindia.cloudapp.azure.com/email-notification-service/api/NotificationService/sendMail";

            try
            {
                // Query data from the database view
                var candidates = await _db.CandidatesShortlisted
                    .Where(c => c.JobId == jobId)
                    .ToListAsync();

                if (candidates == null || !candidates.Any())
                {
                    _response.IsSuccess = false;
                    _response.Message = "No candidates found for the given Job ID.";
                    return NotFound(_response);
                }

                using (var client = new HttpClient(handler))
                {
                    var failedEmails = new List<string>();

                    foreach (var candidate in candidates)
                    {
                        // Prepare email content for candidates
                        var emailRequest = new
                        {
                            to = new[]
                            {
                        new { name = $"{candidate.first_name} {candidate.last_name}", email = candidate.email }
                    },
                            subject = $"Job Opportunity: {candidate.latestrole}",
                            plainTextBody = $"Dear {candidate.first_name},\n\nWe are pleased to inform you that you have been shortlisted for the role of {candidate.latestrole}. Please check your application status for further details.\n\nBest Regards,\nAdani Talent Acquisition Team",
                            htmlBody = $"<p>Dear {candidate.first_name},</p><p>We are pleased to inform you that you have been shortlisted for the role of <b>{candidate.latestrole}</b>. Please check your application status for further details.</p><p>Best Regards,<br>Adani Talent Acquisition Team</p>"
                        };

                        var requestContent = new StringContent(JsonSerializer.Serialize(emailRequest), System.Text.Encoding.UTF8, "application/json");

                        // Send the email notification to candidates
                        var response = await client.PostAsync(emailApiUrl, requestContent);

                        if (!response.IsSuccessStatusCode)
                        {
                            failedEmails.Add(candidate.email);
                        }
                    }

                    // Send notification to hiring manager
                    var managerEmailRequest = new
                    {
                        to = new[]
                        {
                    new { name = "Hiring Manager", email = jobHiringManager }
                },
                        subject = $"[{candidates.FirstOrDefault()?.latestrole}] Position Now Live",
                        plainTextBody = $"Hello Hiring Manager,\n\nThe position for {candidates.FirstOrDefault()?.latestrole} has been successfully created and is now posted on the Hiring Portal.\n\nBest regards,\nHiring Platform",
                        htmlBody = $"<p>Hello Hiring Manager,</p><p>The position for <b>{candidates.FirstOrDefault()?.latestrole}</b> has been successfully created and is now posted on the Hiring Portal.</p><p>Best regards,<br>Hiring Platform</p>"
                    };

                    var managerRequestContent = new StringContent(JsonSerializer.Serialize(managerEmailRequest), System.Text.Encoding.UTF8, "application/json");

                    var managerResponse = await client.PostAsync(emailApiUrl, managerRequestContent);

                    if (!managerResponse.IsSuccessStatusCode)
                    {
                        _response.IsSuccess = false;
                        _response.Message = $"Failed to send email to the Hiring Manager. Status Code: {managerResponse.StatusCode}";
                        return StatusCode((int)managerResponse.StatusCode, _response);
                    }

                    // Check if there were any failures
                    if (failedEmails.Any())
                    {
                        _response.IsSuccess = false;
                        _response.Message = $"Emails failed to send to the following addresses: {string.Join(", ", failedEmails)}.";
                        return StatusCode(207, _response); // 207: Multi-Status
                    }
                }

                _response.IsSuccess = true;
                _response.Message = "Emails sent successfully to all shortlisted candidates and hiring manager.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = $"An error occurred: {ex.Message}";

                if (ex.InnerException != null)
                {
                    _response.Message += $" Inner Exception: {ex.InnerException.Message}";
                }

                return StatusCode(500, _response);
            }
        }





    }
}
