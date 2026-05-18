using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models.BusinessModels;
using PRN232.LMS.Services.Models.BusinessModels.Course;
using PRN232.LMS.Services.Models.BusinessModels.Semester;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN232.LMS.Services.Implements
{
    public class SemesterService : ISemesterService
    {
        private readonly Repositories.Interfaces.IGenericRepository<Semester> _semesterRepository;
        private readonly Repositories.Interfaces.IGenericRepository<Course> _courseRepository;

        public SemesterService(
            Repositories.Interfaces.IGenericRepository<Semester> semesterRepository,
            Repositories.Interfaces.IGenericRepository<Course> courseRepository)
        {
            _semesterRepository = semesterRepository;
            _courseRepository = courseRepository;
        }

        public async Task<PagedResultModel<SemesterModel>> GetAllAsync(
            string? search,
            string? sort,
            string? expand,
            int page,
            int pageSize)
        {
            var includeCourses = ParseCsv(expand).Contains("courses");
            var query = _semesterRepository.Query();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(s => s.SemesterName.Contains(search));
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var semesters = ApplySort(await query.ToListAsync(), sort)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var courses = includeCourses
                ? await _courseRepository.Query()
                    .Where(c => semesters.Select(s => s.SemesterId).Contains(c.SemesterId))
                    .ToListAsync()
                : new List<Course>();

            var items = semesters
                .Select(s => MapSemester(s, courses.Where(c => c.SemesterId == s.SemesterId), includeCourses))
                .ToList();

            return new PagedResultModel<SemesterModel>
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

        public async Task<SemesterModel?> GetByIdAsync(int id)
        {
            var semester = await _semesterRepository.GetByIdAsync(id);
            if (semester == null)
            {
                return null;
            }

            var courses = await _courseRepository.Query()
                .Where(c => c.SemesterId == id)
                .ToListAsync();

            return MapSemester(semester, courses, includeCourses: true);
        }

        private static HashSet<string> ParseCsv(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? new HashSet<string>()
                : value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(x => x.ToLower())
                    .ToHashSet();
        }

        private static IEnumerable<Semester> ApplySort(IEnumerable<Semester> semesters, string? sort)
        {
            var fields = string.IsNullOrWhiteSpace(sort)
                ? new[] { "semesterid" }
                : sort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            IOrderedEnumerable<Semester>? ordered = null;

            foreach (var rawField in fields)
            {
                var descending = rawField.StartsWith("-");
                var field = rawField.TrimStart('-').ToLower();
                Func<Semester, object> keySelector = field switch
                {
                    "semestername" => s => s.SemesterName,
                    "startdate" => s => s.StartDate,
                    "enddate" => s => s.EndDate,
                    _ => s => s.SemesterId
                };

                ordered = ordered == null
                    ? descending ? semesters.OrderByDescending(keySelector) : semesters.OrderBy(keySelector)
                    : descending ? ordered.ThenByDescending(keySelector) : ordered.ThenBy(keySelector);
            }

            return ordered ?? semesters.OrderBy(s => s.SemesterId);
        }

        private static SemesterModel MapSemester(Semester semester, IEnumerable<Course> courses, bool includeCourses)
        {
            return new SemesterModel
            {
                SemesterId = semester.SemesterId,
                SemesterName = semester.SemesterName,
                StartDate = semester.StartDate,
                EndDate = semester.EndDate,
                Courses = includeCourses
                    ? courses.Select(c => new CourseModel
                    {
                        CourseId = c.CourseId,
                        CourseName = c.CourseName,
                        SemesterId = c.SemesterId
                    }).ToList()
                    : new List<CourseModel>()
            };
        }
    }
}
