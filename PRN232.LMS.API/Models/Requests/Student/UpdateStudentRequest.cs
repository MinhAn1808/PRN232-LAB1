using System.ComponentModel.DataAnnotations;

namespace PRN232.LMS.API.Models.Requests.Student
{
    public class UpdateStudentRequest
    {
        public string? FullName { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage ="Invalid email")]
        public string? Email { get; set; } = string.Empty;

        public DateTime? DateOfBirth { get; set; }
    }
}
