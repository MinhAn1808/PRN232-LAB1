using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Helpers;
using PRN232.LMS.API.Models.Responses;
using PRN232.LMS.Services.Interfaces;

namespace PRN232.LMS.API.Controllers
{
    [Route("api/semesters")]
    [ApiController]
    public class SemesterController : ControllerBase
    {
        private readonly ISemesterService _semesterService;

        public SemesterController(ISemesterService semesterService)
        {
            _semesterService = semesterService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(BaseResponse<PagedResult<object>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSemesters(
            [FromQuery] string? search,
            [FromQuery] string? sort,
            [FromQuery] string? fields,
            [FromQuery] string? expand,
            [FromQuery] string? sortBy = "semesterId",
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

            var result = await _semesterService.GetAllAsync(
                search,
                ResolveSort(sort, sortBy, sortDescending),
                expand,
                page,
                pageSize);

            var semesterResponses = result.Items.Select(ResponseMapper.ToSemesterResponse).ToList();

            return Ok(new BaseResponse<PagedResult<object>>
            {
                Success = true,
                Message = "Get semester list successfully",
                Data = new PagedResult<object>
                {
                    Items = FieldSelectionHelper.SelectFields(semesterResponses, fields),
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
        [ProducesResponseType(typeof(BaseResponse<SemesterResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<SemesterResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSemester([FromRoute] int id)
        {
            var semester = await _semesterService.GetByIdAsync(id);
            if (semester == null)
            {
                return NotFound(new BaseResponse<SemesterResponse>
                {
                    Success = false,
                    Message = "Semester is not found",
                    Data = null,
                    Errors = null
                });
            }

            return Ok(new BaseResponse<SemesterResponse>
            {
                Success = true,
                Message = "Get semester successfully",
                Data = ResponseMapper.ToSemesterResponse(semester),
                Errors = null
            });
        }

        private static string ResolveSort(string? sort, string? sortBy, bool sortDescending)
        {
            if (!string.IsNullOrWhiteSpace(sort))
            {
                return sort;
            }

            var field = string.IsNullOrWhiteSpace(sortBy) ? "semesterId" : sortBy;
            return sortDescending ? $"-{field}" : field;
        }
    }
}
