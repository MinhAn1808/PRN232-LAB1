using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN232.LMS.Services.Models.BusinessModels
{
    public class PagedResultModel<T>
    {
        public List<T> Items { get; set; } = new();

        public PaginationModel Pagination { get; set; } = new();
    }
}
