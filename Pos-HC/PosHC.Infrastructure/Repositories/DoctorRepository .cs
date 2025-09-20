using Microsoft.EntityFrameworkCore;
using PosHC.Application.Interfaces;
using PosHC.Domain.Entities;
using PosHC.Infrastructure.Persistence;

namespace PosHC.Infrastructure.Repositories
{
    public class DoctorRepository : IDoctorRepository
    {
        private readonly ApplicationDbContext _context;

        public DoctorRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Doctor>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Doctor.AsNoTracking().ToListAsync(cancellationToken);
        }
    }
}