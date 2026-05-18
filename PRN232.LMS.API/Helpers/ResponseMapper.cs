using PRN232.LMS.API.Models.Responses;
using PRN232.LMS.Services.Models.BusinessModels.Course;
using PRN232.LMS.Services.Models.BusinessModels.Enrollment;
using PRN232.LMS.Services.Models.BusinessModels.Semester;
using PRN232.LMS.Services.Models.BusinessModels.Student;
using PRN232.LMS.Services.Models.BusinessModels.Subject;

namespace PRN232.LMS.API.Helpers
{
    public static class ResponseMapper
    {
        public static StudentResponse ToStudentResponse(StudentModel student)
        {
            return new StudentResponse
            {
                StudentId = student.StudentId,
                FullName = student.FullName,
                Email = student.Email,
                DateOfBirth = student.DateOfBirth,
                Enrollments = student.Enrollments.Select(ToEnrollmentResponse).ToList()
            };
        }

        public static CourseResponse ToCourseResponse(CourseModel course)
        {
            return new CourseResponse
            {
                CourseId = course.CourseId,
                CourseName = course.CourseName,
                SemesterId = course.SemesterId,
                Semester = course.Semester == null ? null : ToSemesterResponse(course.Semester)
            };
        }

        public static EnrollmentResponse ToEnrollmentResponse(EnrollmentModel enrollment)
        {
            return new EnrollmentResponse
            {
                EnrollmentId = enrollment.EnrollmentId,
                StudentId = enrollment.StudentId,
                CourseId = enrollment.CourseId,
                EnrollDate = enrollment.EnrollDate,
                Status = enrollment.Status,
                Student = enrollment.Student == null ? null : ToStudentResponse(enrollment.Student),
                Course = enrollment.Course == null ? null : ToCourseResponse(enrollment.Course)
            };
        }

        public static SemesterResponse ToSemesterResponse(SemesterModel semester)
        {
            return new SemesterResponse
            {
                SemesterId = semester.SemesterId,
                SemesterName = semester.SemesterName,
                StartDate = semester.StartDate,
                EndDate = semester.EndDate,
                Courses = semester.Courses.Select(ToCourseResponseWithoutSemester).ToList()
            };
        }

        public static SubjectResponse ToSubjectResponse(SubjectModel subject)
        {
            return new SubjectResponse
            {
                SubjectId = subject.SubjectId,
                SubjectCode = subject.SubjectCode,
                SubjectName = subject.SubjectName,
                Credit = subject.Credit
            };
        }

        private static CourseResponse ToCourseResponseWithoutSemester(CourseModel course)
        {
            return new CourseResponse
            {
                CourseId = course.CourseId,
                CourseName = course.CourseName,
                SemesterId = course.SemesterId
            };
        }
    }
}
