using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN232.LMS.Repositories.Implements
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly LMSDbContext _context;
        public UnitOfWork(LMSDbContext context)
        {
            _context = context;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
