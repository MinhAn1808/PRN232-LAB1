using PRN232.LMS.Services.Models.BusinessModels;
using PRN232.LMS.Services.Models.BusinessModels.Enrollment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN232.LMS.Services.Interfaces
{
    public interface IEnrollmentService
    {
        Task<PagedResultModel<EnrollmentModel>> GetAllAsync(
            string? search,
            string? sort,
            string? expand,
            int page,
            int pageSize);

        Task<EnrollmentModel?> GetByIdAsync(int id);

        Task<EnrollmentModel?> AddEnrollmentAsync(CreateEnrollmentRequestModel request);
    }
}
