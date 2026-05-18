using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Helpers;
using PRN232.LMS.API.Models.Responses;
using PRN232.LMS.Services.Interfaces;

namespace PRN232.LMS.API.Controllers
{
    [Route("api/subjects")]
    [ApiController]
    public class SubjectController : ControllerBase
    {
        private readonly ISubjectService _subjectService;

        public SubjectController(ISubjectService subjectService)
        {
            _subjectService = subjectService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(BaseResponse<PagedResult<object>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSubjects(
            [FromQuery] string? search,
            [FromQuery] string? sort,
            [FromQuery] string? fields,
            [FromQuery] string? sortBy = "subjectId",
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

            var result = await _subjectService.GetAllAsync(
                search,
                ResolveSort(sort, sortBy, sortDescending),
                page,
                pageSize);

            var subjectResponses = result.Items.Select(ResponseMapper.ToSubjectResponse).ToList();

            return Ok(new BaseResponse<PagedResult<object>>
            {
                Success = true,
                Message = "Get subject list successfully",
                Data = new PagedResult<object>
                {
                    Items = FieldSelectionHelper.SelectFields(subjectResponses, fields),
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
        [ProducesResponseType(typeof(BaseResponse<SubjectResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<SubjectResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSubject([FromRoute] int id)
        {
            var subject = await _subjectService.GetByIdAsync(id);
            if (subject == null)
            {
                return NotFound(new BaseResponse<SubjectResponse>
                {
                    Success = false,
                    Message = "Subject is not found",
                    Data = null,
                    Errors = null
                });
            }

            return Ok(new BaseResponse<SubjectResponse>
            {
                Success = true,
                Message = "Get subject successfully",
                Data = ResponseMapper.ToSubjectResponse(subject),
                Errors = null
            });
        }

        private static string ResolveSort(string? sort, string? sortBy, bool sortDescending)
        {
            if (!string.IsNullOrWhiteSpace(sort))
            {
                return sort;
            }

            var field = string.IsNullOrWhiteSpace(sortBy) ? "subjectId" : sortBy;
            return sortDescending ? $"-{field}" : field;
        }
    }
}
