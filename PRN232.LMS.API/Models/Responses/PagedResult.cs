namespace PRN232.LMS.API.Models.Responses
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();

        public PaginationMetadata Pagination { get; set; } = new();
    
    }   
}
