using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
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
    public class CourseService : ICourseService
    {
        private readonly Repositories.Interfaces.IGenericRepository<Course> _courseRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CourseService(Repositories.Interfaces.IGenericRepository<Course> courseRepository, IUnitOfWork unitOfWork)
        {
            _courseRepository = courseRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResultModel<CourseModel>> GetAllAsync(
            string? search,
            string? sort,
            string? expand,
            int page,
            int pageSize)
        {
            var includes = ParseCsv(expand);
            var includeSemester = includes.Contains("semester");
            var query = _courseRepository.Query();

            if (includeSemester)
            {
                query = query.Include(c => c.Semester);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => x.CourseName.Contains(search));
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var courses = await query.ToListAsync();
            var items = ApplySort(courses, sort)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => MapCourse(c, includeSemester))
                .ToList();

            return new PagedResultModel<CourseModel>
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

        public async Task<CourseModel?> GetByIdAsync(int id)
        {
            var course = await _courseRepository.Query()
                .Include(c => c.Semester)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null)
            {
                return null;
            }

            return MapCourse(course, includeSemester: true);
        }

        private static HashSet<string> ParseCsv(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? new HashSet<string>()
                : value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(x => x.ToLower())
                    .ToHashSet();
        }

        private static IEnumerable<Course> ApplySort(IEnumerable<Course> courses, string? sort)
        {
            var fields = string.IsNullOrWhiteSpace(sort)
                ? new[] { "courseid" }
                : sort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            IOrderedEnumerable<Course>? ordered = null;

            foreach (var rawField in fields)
            {
                var descending = rawField.StartsWith("-");
                var field = rawField.TrimStart('-').ToLower();
                Func<Course, object> keySelector = field switch
                {
                    "coursename" => c => c.CourseName,
                    "semesterid" => c => c.SemesterId,
                    _ => c => c.CourseId
                };

                ordered = ordered == null
                    ? descending ? courses.OrderByDescending(keySelector) : courses.OrderBy(keySelector)
                    : descending ? ordered.ThenByDescending(keySelector) : ordered.ThenBy(keySelector);
            }

            return ordered ?? courses.OrderBy(c => c.CourseId);
        }

        private static CourseModel MapCourse(Course course, bool includeSemester)
        {
            return new CourseModel
            {
                CourseId = course.CourseId,
                CourseName = course.CourseName,
                SemesterId = course.SemesterId,
                Semester = includeSemester && course.Semester != null
                    ? new SemesterModel
                    {
                        SemesterId = course.Semester.SemesterId,
                        SemesterName = course.Semester.SemesterName,
                        StartDate = course.Semester.StartDate,
                        EndDate = course.Semester.EndDate
                    }
                    : null
            };
        }
    }
}
