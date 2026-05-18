using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Helpers;
using PRN232.LMS.API.Models.Responses;
using PRN232.LMS.Services.Interfaces;

namespace PRN232.LMS.API.Controllers
{
    [Route("api/courses")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public CourseController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(BaseResponse<PagedResult<object>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllCoursesAsync(
            [FromQuery] string? search,
            [FromQuery] string? sort,
            [FromQuery] string? fields,
            [FromQuery] string? expand,
            [FromQuery] string? sortBy = "courseId",
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

            var result = await _courseService.GetAllAsync(
                search,
                ResolveSort(sort, sortBy, sortDescending),
                expand,
                page,
                pageSize);

            var courseResponses = result.Items.Select(ResponseMapper.ToCourseResponse).ToList();

            return Ok(new BaseResponse<PagedResult<object>>
            {
                Success = true,
                Message = "Get course successfully",
                Data = new PagedResult<object>
                {
                    Items = FieldSelectionHelper.SelectFields(courseResponses, fields),
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
        [ProducesResponseType(typeof(BaseResponse<CourseResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<CourseResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCourse(
            [FromRoute] int id)
        {
            var course = await _courseService.GetByIdAsync(id);
            if (course == null)
            {
                return NotFound(new BaseResponse<CourseResponse>
                {
                    Success = false,
                    Message = "Course is not found",
                    Data = null,
                    Errors = null
                });
            }

            return Ok(new BaseResponse<CourseResponse>
            {
                Success = true,
                Message = "Get course successfully",
                Data = ResponseMapper.ToCourseResponse(course),
                Errors = null
            });
        }

        private static string ResolveSort(string? sort, string? sortBy, bool sortDescending)
        {
            if (!string.IsNullOrWhiteSpace(sort))
            {
                return sort;
            }

            var field = string.IsNullOrWhiteSpace(sortBy) ? "courseId" : sortBy;
            return sortDescending ? $"-{field}" : field;
        }
    }
}
