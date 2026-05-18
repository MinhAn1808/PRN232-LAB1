using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PRN232.LMS.Services.Models.BusinessModels.Course;
using PRN232.LMS.Services.Models.BusinessModels.Student;

namespace PRN232.LMS.Services.Models.BusinessModels.Enrollment
{
    public class EnrollmentModel
    {
        public int EnrollmentId { get; set; }
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public DateTime EnrollDate { get; set; }
        public string Status { get; set; } = null!;
        public StudentModel? Student { get; set; }
        public CourseModel? Course { get; set; }
    }
}
