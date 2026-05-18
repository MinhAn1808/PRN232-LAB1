using System.ComponentModel.DataAnnotations;

namespace PRN232.LMS.API.Models.Requests.Student
{
    public class CreateStudentRequest
    {
        [Required(ErrorMessage ="Full name is required")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email name is required")]
        [EmailAddress(ErrorMessage ="Email is invalid")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of birth is required")]
        public DateTime DateOfBirth { get; set; }

    }
}
