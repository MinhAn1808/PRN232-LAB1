using System.ComponentModel.DataAnnotations;

namespace PRN232.LMS.API.Models.Requests.Enrollment
{
    public class CreateEnrollmentRequest
    {
        [Required(ErrorMessage = "Student id is required")]
        public int? StudentId { get; set; }

        [Required(ErrorMessage = "Course id is required")]
        public int? CourseId { get; set; }

        [Required(ErrorMessage = "Enroll date is required")]
        public DateTime? EnrollDate { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; } = string.Empty;
    }
}
