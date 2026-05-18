using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN232.LMS.Services.Models.BusinessModels.Student
{
    public class UpdateStudentRequestModel
    {
        public string? FullName { get; set; } = string.Empty;

        public string? Email { get; set; } = string.Empty;

        public DateTime? DateOfBirth { get; set; }
    }
}
