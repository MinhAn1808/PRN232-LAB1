using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PRN232.LMS.Services.Models.BusinessModels.Enrollment;

namespace PRN232.LMS.Services.Models.BusinessModels.Student
{
    public class StudentModel
    {
        public int StudentId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }
        public List<EnrollmentModel> Enrollments { get; set; } = new();
    }
}
