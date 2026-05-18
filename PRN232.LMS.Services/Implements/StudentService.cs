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
    public class StudentService : IStudentService
    {
        private readonly Repositories.Interfaces.IGenericRepository<Student> _studentRepository;
        private readonly IUnitOfWork _unitOfWork;

        public StudentService(Repositories.Interfaces.IGenericRepository<Student> studentRepository, IUnitOfWork unitOfWork)
        {
            _studentRepository = studentRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResultModel<StudentModel>> GetAllAsync(
            string? search,
            string? sort,
            string? expand,
            int page,
            int pageSize)
        {
            var includes = ParseCsv(expand);
            var includeEnrollments = includes.Contains("enrollments");
            var query = _studentRepository.Query();

            if (includeEnrollments)
            {
                query = query
                    .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Course)
                    .ThenInclude(c => c.Semester);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(s =>
                    s.FullName.Contains(search) ||
                    s.Email.Contains(search));
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var students = await query.ToListAsync();
            var items = ApplySort(students, sort)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => MapStudent(s, includeEnrollments))
                .ToList();

            return new PagedResultModel<StudentModel>
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

        public async Task<StudentModel?> GetByIdAsync(int id)
        {
            var student = await _studentRepository.Query()
                .Include(s => s.Enrollments)
                .ThenInclude(e => e.Course)
                .ThenInclude(c => c.Semester)
                .FirstOrDefaultAsync(s => s.StudentId == id);

            if (student == null)
            {
                return null;
            }

            return MapStudent(student, includeEnrollments: true);
        }

        public async Task<StudentModel> AddStudentAsync(CreateStudentRequestModel request)
        {
            var student = new Student
            {
                FullName = request.FullName,
                Email = request.Email,
                DateOfBirth = request.DateOfBirth,
            };

            await _studentRepository.AddAsync(student);
            await _unitOfWork.SaveChangesAsync();

            return MapStudent(student, includeEnrollments: false);
        }

        public async Task<StudentModel?> UpdateStudentAsync(UpdateStudentRequestModel request, int id)
        {
            var student = await _studentRepository.GetByIdAsync(id);
            if (student == null)
            {
                return null;
            }
            if (!string.IsNullOrWhiteSpace(request.FullName))
            {
                student.FullName = request.FullName;
            }
            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                student.Email = request.Email;
            }
            if (request.DateOfBirth.HasValue)
            {
                student.DateOfBirth = request.DateOfBirth.Value;
            }
            _studentRepository.Update(student);
            await _unitOfWork.SaveChangesAsync();

            return MapStudent(student, includeEnrollments: false);
        }

        public async Task<bool> DeleteStudentAsync(int id)
        {
            var student = await _studentRepository.GetByIdAsync(id);
            if (student == null)
            {
                return false;
            }
            _studentRepository.Delete(student);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        private static HashSet<string> ParseCsv(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? new HashSet<string>()
                : value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(x => x.ToLower())
                    .ToHashSet();
        }

        private static IEnumerable<Student> ApplySort(IEnumerable<Student> students, string? sort)
        {
            var fields = string.IsNullOrWhiteSpace(sort)
                ? new[] { "studentid" }
                : sort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            IOrderedEnumerable<Student>? ordered = null;

            foreach (var rawField in fields)
            {
                var descending = rawField.StartsWith("-");
                var field = rawField.TrimStart('-').ToLower();
                Func<Student, object> keySelector = field switch
                {
                    "fullname" => s => s.FullName,
                    "email" => s => s.Email,
                    "dateofbirth" => s => s.DateOfBirth,
                    _ => s => s.StudentId
                };

                ordered = ordered == null
                    ? descending ? students.OrderByDescending(keySelector) : students.OrderBy(keySelector)
                    : descending ? ordered.ThenByDescending(keySelector) : ordered.ThenBy(keySelector);
            }

            return ordered ?? students.OrderBy(s => s.StudentId);
        }

        private static StudentModel MapStudent(Student student, bool includeEnrollments)
        {
            return new StudentModel
            {
                StudentId = student.StudentId,
                FullName = student.FullName,
                Email = student.Email,
                DateOfBirth = student.DateOfBirth,
                Enrollments = includeEnrollments
                    ? student.Enrollments.Select(e => new EnrollmentModel
                    {
                        EnrollmentId = e.EnrollmentId,
                        StudentId = e.StudentId,
                        CourseId = e.CourseId,
                        EnrollDate = e.EnrollDate,
                        Status = e.Status,
                        Course = e.Course == null ? null : new CourseModel
                        {
                            CourseId = e.Course.CourseId,
                            CourseName = e.Course.CourseName,
                            SemesterId = e.Course.SemesterId,
                            Semester = e.Course.Semester == null ? null : new SemesterModel
                            {
                                SemesterId = e.Course.Semester.SemesterId,
                                SemesterName = e.Course.Semester.SemesterName,
                                StartDate = e.Course.Semester.StartDate,
                                EndDate = e.Course.Semester.EndDate
                            }
                        }
                    }).ToList()
                    : new List<EnrollmentModel>()
            };
        }
    }
}
