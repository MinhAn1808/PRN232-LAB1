using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Helpers;
using PRN232.LMS.API.Models.Requests.Student;
using PRN232.LMS.API.Models.Responses;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models.BusinessModels.Student;

namespace PRN232.LMS.API.Controllers
{
    [Route("api/students")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _studentService;

        public StudentController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(BaseResponse<PagedResult<object>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetStudents(
            [FromQuery] string? search,
            [FromQuery] string? sort,
            [FromQuery] string? fields,
            [FromQuery] string? expand,
            [FromQuery] string? sortBy = "studentId",
            [FromQuery] bool sortDescending = false,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (page <= 0 || pageSize <= 0)
            {
                return BadRequest(new BaseResponse<object>
                {
                    Success = false,
                    Message = "Page and pageSize must be greater than 0",
                    Data = null,
                    Errors = null
                });
            }

            var result = await _studentService.GetAllAsync(
                search,
                ResolveSort(sort, sortBy, sortDescending),
                expand,
                page,
                pageSize
            );

            var studentResponses = result.Items.Select(ResponseMapper.ToStudentResponse).ToList();

            return Ok(new BaseResponse<PagedResult<object>>
            {
                Success = true,
                Message = "Get student list successfully",
                Data = new PagedResult<object>
                {
                    Items = FieldSelectionHelper.SelectFields(studentResponses, fields),
                    Pagination = new PaginationMetadata
                    {
                        Page = result.Pagination.Page,
                        PageSize = result.Pagination.PageSize,
                        TotalItems = result.Pagination.TotalItems,
                        TotalPages = result.Pagination.TotalPages
                    }
                },
                Errors = null
            });
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResponse<StudentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<StudentResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetStudent(
            [FromRoute] int id)
        {
            var student = await _studentService.GetByIdAsync(id);
            if (student == null)
            {
                return NotFound(new BaseResponse<StudentResponse>
                {
                    Success = false,
                    Message = "Student is not found",
                    Data = null,
                    Errors = null
                });
            }

            return Ok(new BaseResponse<StudentResponse>
            {
                Success = true,
                Message = "Get student successfully",
                Data = ResponseMapper.ToStudentResponse(student),
                Errors = null
            });
        }

        [HttpPost]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddStudent(
            [FromBody] CreateStudentRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value != null && x.Value.Errors.Count > 0)
                    .Select(x => new
                    {
                        Field = x.Key,
                        Errors = x.Value!.Errors.Select(e => e.ErrorMessage)
                    });

                return BadRequest(new BaseResponse<object>
                {
                    Success = false,
                    Message = "Validation failed",
                    Data = null,
                    Errors = errors
                });
            }

            if (request.DateOfBirth > DateTime.Now)
            {
                return BadRequest(new BaseResponse<object>
                {
                    Success = false,
                    Message = "Date of birth is invalid",
                    Data = null,
                    Errors = new[]
                    {
                        new
                        {
                            Field = "Date of birth",
                            Errors = new[]{ "Date of birth can not in the future" }
                        }
                    }
                });
            }

            var student = await _studentService.AddStudentAsync(new CreateStudentRequestModel
            {
                FullName = request.FullName,
                Email = request.Email,
                DateOfBirth = request.DateOfBirth,
            });

            return Created("", new BaseResponse<object>
            {
                Success = true,
                Message = "Add student is successfully",
                Data = ResponseMapper.ToStudentResponse(student),
                Errors = null
            });
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(BaseResponse<StudentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<StudentResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateStudent(
            [FromRoute] int id,
            [FromBody] UpdateStudentRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value != null && x.Value.Errors.Count > 0)
                    .Select(x => new
                    {
                        Field = x.Key,
                        Errors = x.Value!.Errors.Select(x => x.ErrorMessage)
                    });

                return BadRequest(new BaseResponse<object>
                {
                    Success = false,
                    Message = "Validation failed",
                    Data = null,
                    Errors = errors
                });
            }

            var student = await _studentService.UpdateStudentAsync(
                new UpdateStudentRequestModel
                {
                    FullName = request.FullName,
                    DateOfBirth = request.DateOfBirth,
                    Email = request.Email,
                },
                id);

            if (student == null)
            {
                return NotFound(new BaseResponse<StudentResponse>
                {
                    Success = false,
                    Message = "Student is not found",
                    Data = null,
                    Errors = null
                });
            }

            return Ok(new BaseResponse<StudentResponse>
            {
                Success = true,
                Message = "Update student successfully",
                Data = ResponseMapper.ToStudentResponse(student),
                Errors = null
            });
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteStudent(
            [FromRoute] int id)
        {
            var result = await _studentService.DeleteStudentAsync(id);
            if (!result)
            {
                return NotFound(new BaseResponse<object>
                {
                    Success = false,
                    Message = "Student is not found",
                    Data = null,
                    Errors = null
                });
            }

            return Ok(new BaseResponse<object>
            {
                Success = true,
                Message = "Student deleted",
                Data = null,
                Errors = null
            });
        }

        private static string ResolveSort(string? sort, string? sortBy, bool sortDescending)
        {
            if (!string.IsNullOrWhiteSpace(sort))
            {
                return sort;
            }

            var field = string.IsNullOrWhiteSpace(sortBy) ? "studentId" : sortBy;
            return sortDescending ? $"-{field}" : field;
        }
    }
}
