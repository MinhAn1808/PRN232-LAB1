using PRN232.LMS.Services.Models.BusinessModels;
using PRN232.LMS.Services.Models.BusinessModels.Semester;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN232.LMS.Services.Interfaces
{
    public interface ISemesterService
    {
        Task<PagedResultModel<SemesterModel>> GetAllAsync(
            string? search,
            string? sort,
            string? expand,
            int page,
            int pageSize);

        Task<SemesterModel?> GetByIdAsync(int id);
    }
}
