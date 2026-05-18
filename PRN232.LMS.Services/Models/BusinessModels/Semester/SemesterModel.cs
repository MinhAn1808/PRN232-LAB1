using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PRN232.LMS.Services.Models.BusinessModels.Course;

namespace PRN232.LMS.Services.Models.BusinessModels.Semester
{
    public class SemesterModel
    {
        public int SemesterId { get; set; }
        public string SemesterName { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<CourseModel> Courses { get; set; } = new();
    }
}
