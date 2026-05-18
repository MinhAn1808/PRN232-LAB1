using PRN232.LMS.Services.Models.BusinessModels;
using PRN232.LMS.Services.Models.BusinessModels.Course;
using PRN232.LMS.Services.Models.BusinessModels.Student;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN232.LMS.Services.Interfaces
{
    public interface ICourseService
    {
        Task<PagedResultModel<CourseModel>> GetAllAsync(
        string? search,
        string? sort,
        string? expand,
        int page,
        int pageSize);

        Task<CourseModel?> GetByIdAsync(
            int id);
    }
}
