using PRN232.LMS.Services.Models.BusinessModels;
using PRN232.LMS.Services.Models.BusinessModels.Subject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN232.LMS.Services.Interfaces
{
    public interface ISubjectService
    {
        Task<PagedResultModel<SubjectModel>> GetAllAsync(
            string? search,
            string? sort,
            int page,
            int pageSize);

        Task<SubjectModel?> GetByIdAsync(int id);
    }
}
