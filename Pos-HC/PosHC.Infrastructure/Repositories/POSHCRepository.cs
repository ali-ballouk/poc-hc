using Microsoft.EntityFrameworkCore;
using PosHC.Application.Interfaces;
using PosHC.Domain.Entities;
using PosHC.Infrastructure.Persistence;

namespace PosHC.Infrastructure.Repositories
{
    public class POSHCRepository : IPOSHCRepository
    {
        private readonly ApplicationDbContext _context;

        public POSHCRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Doctor>> GetAllDoctorsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Doctor.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<List<Patient>> GetAllPatientsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Patient.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<List<CatalogItem>> GetAllItemsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.CatalogItem.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<List<Invoice>> GetAllIInvoiceAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Invoice.Include(i => i.Items).AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<Invoice> SaveInvoice(Invoice invoice,CancellationToken cancellationToken = default)
        {

            _context.Invoice.Add(invoice);

            await _context.SaveChangesAsync(cancellationToken);

            return invoice;
        }
    }
}