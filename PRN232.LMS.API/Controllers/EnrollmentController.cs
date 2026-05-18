using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Helpers;
using PRN232.LMS.API.Models.Requests.Enrollment;
using PRN232.LMS.API.Models.Responses;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models.BusinessModels.Enrollment;

namespace PRN232.LMS.API.Controllers
{
    [Route("api/enrollments")]
    [ApiController]
    public class EnrollmentController : ControllerBase
    {
        private readonly IEnrollmentService _enrollmentService;

        public EnrollmentController(IEnrollmentService enrollmentService)
        {
            _enrollmentService = enrollmentService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(BaseResponse<PagedResult<object>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetEnrollments(
            [FromQuery] string? search,
            [FromQuery] string? sort,
            [FromQuery] string? fields,
            [FromQuery] string? expand,
            [FromQuery] string? sortBy = "enrollmentId",
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

            var result = await _enrollmentService.GetAllAsync(
                search,
                ResolveSort(sort, sortBy, sortDescending),
                expand,
                page,
                pageSize);

            var enrollmentResponses = result.Items.Select(ResponseMapper.ToEnrollmentResponse).ToList();

            return Ok(new BaseResponse<PagedResult<object>>
            {
                Success = true,
                Message = "Get enrollment list successfully",
                Data = new PagedResult<object>
                {
                    Items = FieldSelectionHelper.SelectFields(enrollmentResponses, fields),
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
        [ProducesResponseType(typeof(BaseResponse<EnrollmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<EnrollmentResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetEnrollment(
            [FromRoute] int id)
        {
            var enrollment = await _enrollmentService.GetByIdAsync(id);
            if (enrollment == null)
            {
                return NotFound(new BaseResponse<EnrollmentResponse>
                {
                    Success = false,
                    Message = "Enrollment is not found",
                    Data = null,
                    Errors = null
                });
            }

            return Ok(new BaseResponse<EnrollmentResponse>
            {
                Success = true,
                Message = "Get enrollment successfully",
                Data = ResponseMapper.ToEnrollmentResponse(enrollment),
                Errors = null
            });
        }

        [HttpPost]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddEnrollment(
            [FromBody] CreateEnrollmentRequest request)
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

            if (request.EnrollDate > DateTime.Now)
            {
                return BadRequest(new BaseResponse<object>
                {
                    Success = false,
                    Message = "Enroll date is invalid",
                    Data = null,
                    Errors = new[]
                    {
                        new
                        {
                            Field = "Enroll date",
                            Errors = new[] { "Enroll date can not in the future" }
                        }
                    }
                });
            }

            var enrollment = await _enrollmentService.AddEnrollmentAsync(new CreateEnrollmentRequestModel
            {
                StudentId = request.StudentId!.Value,
                CourseId = request.CourseId!.Value,
                EnrollDate = request.EnrollDate!.Value,
                Status = request.Status
            });

            if (enrollment == null)
            {
                return NotFound(new BaseResponse<object>
                {
                    Success = false,
                    Message = "Student or course is not found",
                    Data = null,
                    Errors = null
                });
            }

            return Created("", new BaseResponse<object>
            {
                Success = true,
                Message = "Add enrollment is successfully",
                Data = ResponseMapper.ToEnrollmentResponse(enrollment),
                Errors = null
            });
        }

        private static string ResolveSort(string? sort, string? sortBy, bool sortDescending)
        {
            if (!string.IsNullOrWhiteSpace(sort))
            {
                return sort;
            }

            var field = string.IsNullOrWhiteSpace(sortBy) ? "enrollmentId" : sortBy;
            return sortDescending ? $"-{field}" : field;
        }
    }
}
