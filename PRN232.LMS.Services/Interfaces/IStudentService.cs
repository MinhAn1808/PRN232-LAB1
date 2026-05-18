using PRN232.LMS.Services.Models.BusinessModels;
using PRN232.LMS.Services.Models.BusinessModels.Student;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN232.LMS.Services.Interfaces
{
    public interface IStudentService
    {
        Task<PagedResultModel<StudentModel>> GetAllAsync(
        string? search,
        string? sort,
        string? expand,
        int page,
        int pageSize);
        Task<StudentModel?> GetByIdAsync(
            int id);

        Task<StudentModel> AddStudentAsync(
            CreateStudentRequestModel request);

        Task<StudentModel?> UpdateStudentAsync(
            UpdateStudentRequestModel request,
            int id);
        Task<bool> DeleteStudentAsync(int id);
    }
}
