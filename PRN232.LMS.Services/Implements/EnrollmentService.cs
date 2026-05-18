using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models.BusinessModels;
using PRN232.LMS.Services.Models.BusinessModels.Course;
using PRN232.LMS.Services.Models.BusinessModels.Enrollment;
using PRN232.LMS.Services.Models.BusinessModels.Semester;
using PRN232.LMS.Services.Models.BusinessModels.Student;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN232.LMS.Services.Implements
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly Repositories.Interfaces.IGenericRepository<Enrollment> _enrollmentRepository;
        private readonly Repositories.Interfaces.IGenericRepository<Student> _studentRepository;
        private readonly Repositories.Interfaces.IGenericRepository<Course> _courseRepository;
        private readonly IUnitOfWork _unitOfWork;

        public EnrollmentService(
            Repositories.Interfaces.IGenericRepository<Enrollment> enrollmentRepository,
            Repositories.Interfaces.IGenericRepository<Student> studentRepository,
            Repositories.Interfaces.IGenericRepository<Course> courseRepository,
            IUnitOfWork unitOfWork)
        {
            _enrollmentRepository = enrollmentRepository;
            _studentRepository = studentRepository;
            _courseRepository = courseRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResultModel<EnrollmentModel>> GetAllAsync(
            string? search,
            string? sort,
            string? expand,
            int page,
            int pageSize)
        {
            var includes = ParseCsv(expand);
            var includeStudent = includes.Contains("student");
            var includeCourse = includes.Contains("course");
            var query = _enrollmentRepository.Query();

            if (includeStudent)
            {
                query = query.Include(e => e.Student);
            }

            if (includeCourse)
            {
                query = query
                    .Include(e => e.Course)
                    .ThenInclude(c => c.Semester);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => x.Status.Contains(search));
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var enrollments = await query.ToListAsync();
            var items = ApplySort(enrollments, sort)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => MapEnrollment(e, includeStudent, includeCourse))
                .ToList();

            return new PagedResultModel<EnrollmentModel>
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

        public async Task<EnrollmentModel?> GetByIdAsync(int id)
        {
            var enrollment = await _enrollmentRepository.Query()
                .Include(e => e.Student)
                .Include(e => e.Course)
                .ThenInclude(c => c.Semester)
                .FirstOrDefaultAsync(e => e.EnrollmentId == id);

            if (enrollment == null)
            {
                return null;
            }

            return MapEnrollment(enrollment, includeStudent: true, includeCourse: true);
        }

        public async Task<EnrollmentModel?> AddEnrollmentAsync(CreateEnrollmentRequestModel request)
        {
            var student = await _studentRepository.GetByIdAsync(request.StudentId);
            var course = await _courseRepository.GetByIdAsync(request.CourseId);
            if (student == null || course == null)
            {
                return null;
            }

            var enrollment = new Enrollment
            {
                StudentId = request.StudentId,
                CourseId = request.CourseId,
                EnrollDate = request.EnrollDate,
                Status = request.Status
            };

            await _enrollmentRepository.AddAsync(enrollment);
            await _unitOfWork.SaveChangesAsync();

            return MapEnrollment(enrollment, includeStudent: false, includeCourse: false);
        }

        private static HashSet<string> ParseCsv(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? new HashSet<string>()
                : value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(x => x.ToLower())
                    .ToHashSet();
        }

        private static IEnumerable<Enrollment> ApplySort(IEnumerable<Enrollment> enrollments, string? sort)
        {
            var fields = string.IsNullOrWhiteSpace(sort)
                ? new[] { "enrollmentid" }
                : sort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            IOrderedEnumerable<Enrollment>? ordered = null;

            foreach (var rawField in fields)
            {
                var descending = rawField.StartsWith("-");
                var field = rawField.TrimStart('-').ToLower();
                Func<Enrollment, object> keySelector = field switch
                {
                    "studentid" => e => e.StudentId,
                    "courseid" => e => e.CourseId,
                    "enrolldate" => e => e.EnrollDate,
                    "status" => e => e.Status,
                    _ => e => e.EnrollmentId
                };

                ordered = ordered == null
                    ? descending ? enrollments.OrderByDescending(keySelector) : enrollments.OrderBy(keySelector)
                    : descending ? ordered.ThenByDescending(keySelector) : ordered.ThenBy(keySelector);
            }

            return ordered ?? enrollments.OrderBy(e => e.EnrollmentId);
        }

        private static EnrollmentModel MapEnrollment(Enrollment enrollment, bool includeStudent, bool includeCourse)
        {
            return new EnrollmentModel
            {
                EnrollmentId = enrollment.EnrollmentId,
                StudentId = enrollment.StudentId,
                CourseId = enrollment.CourseId,
                EnrollDate = enrollment.EnrollDate,
                Status = enrollment.Status,
                Student = includeStudent && enrollment.Student != null
                    ? new StudentModel
                    {
                        StudentId = enrollment.Student.StudentId,
                        FullName = enrollment.Student.FullName,
                        Email = enrollment.Student.Email,
                        DateOfBirth = enrollment.Student.DateOfBirth
                    }
                    : null,
                Course = includeCourse && enrollment.Course != null
                    ? new CourseModel
                    {
                        CourseId = enrollment.Course.CourseId,
                        CourseName = enrollment.Course.CourseName,
                        SemesterId = enrollment.Course.SemesterId,
                        Semester = enrollment.Course.Semester == null ? null : new SemesterModel
                        {
                            SemesterId = enrollment.Course.Semester.SemesterId,
                            SemesterName = enrollment.Course.Semester.SemesterName,
                            StartDate = enrollment.Course.Semester.StartDate,
                            EndDate = enrollment.Course.Semester.EndDate
                        }
                    }
                    : null
            };
        }
    }
}
