using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models.BusinessModels;
using PRN232.LMS.Services.Models.BusinessModels.Subject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN232.LMS.Services.Implements
{
    public class SubjectService : ISubjectService
    {
        private readonly Repositories.Interfaces.IGenericRepository<Subject> _subjectRepository;

        public SubjectService(Repositories.Interfaces.IGenericRepository<Subject> subjectRepository)
        {
            _subjectRepository = subjectRepository;
        }

        public async Task<PagedResultModel<SubjectModel>> GetAllAsync(
            string? search,
            string? sort,
            int page,
            int pageSize)
        {
            var query = _subjectRepository.Query();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(s =>
                    s.SubjectCode.Contains(search) ||
                    s.SubjectName.Contains(search));
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var subjects = await query.ToListAsync();
            var items = ApplySort(subjects, sort)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(MapSubject)
                .ToList();

            return new PagedResultModel<SubjectModel>
            {
                Items = items,
                Pagination = new PaginationModel
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages
                }
            };
        }

        public async Task<SubjectModel?> GetByIdAsync(int id)
        {
            var subject = await _subjectRepository.GetByIdAsync(id);
            return subject == null ? null : MapSubject(subject);
        }

        private static IEnumerable<Subject> ApplySort(IEnumerable<Subject> subjects, string? sort)
        {
            var fields = string.IsNullOrWhiteSpace(sort)
                ? new[] { "subjectid" }
                : sort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            IOrderedEnumerable<Subject>? ordered = null;

            foreach (var rawField in fields)
            {
                var descending = rawField.StartsWith("-");
                var field = rawField.TrimStart('-').ToLower();
                Func<Subject, object> keySelector = field switch
                {
                    "subjectcode" => s => s.SubjectCode,
                    "subjectname" => s => s.SubjectName,
                    "credit" => s => s.Credit,
                    _ => s => s.SubjectId
                };

                ordered = ordered == null
                    ? descending ? subjects.OrderByDescending(keySelector) : subjects.OrderBy(keySelector)
                    : descending ? ordered.ThenByDescending(keySelector) : ordered.ThenBy(keySelector);
            }

            return ordered ?? subjects.OrderBy(s => s.SubjectId);
        }

        private static SubjectModel MapSubject(Subject subject)
        {
            return new SubjectModel
            {
                SubjectId = subject.SubjectId,
                SubjectCode = subject.SubjectCode,
                SubjectName = subject.SubjectName,
                Credit = subject.Credit
            };
        }
    }
}
