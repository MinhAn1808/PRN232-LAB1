using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PRN232.LMS.Services.Models.BusinessModels.Semester;

namespace PRN232.LMS.Services.Models.BusinessModels.Course
{
    public class CourseModel
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = null!;
        public int SemesterId { get; set; }
        public SemesterModel? Semester { get; set; }
    }
}
