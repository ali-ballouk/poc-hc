using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;
using PosHC.Domain.Entities;

namespace PosHC.Application.Services
{
    public class InvoiceService : IInvoiceService
    {

        private readonly IPOSHCRepository _invoiceRepository;
        private readonly IDoctorService _doctorService;
        private readonly IPatientService _patientService;
        private readonly ICatalogItemService _catalogItemService;


        public InvoiceService(IPOSHCRepository invoiceRepository, IDoctorService doctorService, ICatalogItemService catalogItemService, IPatientService patientService)
        {
            _invoiceRepository = invoiceRepository;
            _doctorService = doctorService;
            _catalogItemService = catalogItemService;
            _patientService = patientService;
        }

        public async Task<InvoiceResultDto> SaveInvoiceAsync(CreateInvoiceDto dto, CancellationToken cancellationToken = default)
        {
            // 1. Get Doctor
            var doctor =  _doctorService.GetDoctor(dto.DoctorId);
            // 2. Create Invoice
            var invoice = new Invoice
            {
                Id = Guid.NewGuid(),
                DoctorId = dto.DoctorId,
                PatientId = dto.PatientId,
                DoctorFee = doctor.Fee, // snapshot
                Discount = dto.Discount,
                CreatedAt = DateTime.Now
            };

            // 3. Create Invoice Items
            foreach (var itemDto in dto.Items)
            {
                var catalogItem = await _catalogItemService.GetCatalogItem(itemDto.CatalogItemId, cancellationToken);

                if (catalogItem == null)
                    throw new Exception($"Catalog item {itemDto.CatalogItemId} not found.");

                var invoiceItem = new InvoiceItem
                {
                    Id = Guid.NewGuid(),
                    InvoiceId = invoice.Id,
                    CatalogItemId = catalogItem.Id,
                    Quantity = itemDto.Quantity,
                    UnitPrice = catalogItem.UnitPrice
                };

                invoice.Items.Add(invoiceItem);
            }


            var invoiceCreated = await _invoiceRepository.SaveInvoice(invoice, cancellationToken);

            // 5. Return DTO
            var subtotal = invoice.DoctorFee + invoice.Items.Sum(i => i.Quantity * i.UnitPrice);
            var total = subtotal - (invoice.Discount ?? 0);

            return new InvoiceResultDto
            {
                InvoiceId = invoiceCreated.Id,
                Total = invoiceCreated.Total
            };
        }


        public async Task<List<InvoiceDto>> GetAllInvoicesDto(CancellationToken cancellationToken = default)
        {
            var invoices = await _invoiceRepository.GetAllIInvoiceAsync(cancellationToken);
            return invoices.Select(InvoiceDtoMapper).OrderByDescending(dto => dto.InvoiceDate).ToList();
        }
       

        InvoiceDto InvoiceDtoMapper(Invoice invoice)
        {
            var doctor =  _doctorService.GetDoctor(invoice.DoctorId);
            var patient =  _patientService.GetPatient(invoice.PatientId);
            return new InvoiceDto
            {
                InvoiceId = invoice.Id,
                DoctorId = invoice.DoctorId,
                DoctorName = doctor.FullName,
                PatientId = invoice.PatientId,
                PatientName = patient.FullName,
                InvoiceDate = invoice.CreatedAt,
                Discount = invoice.Discount,
                DoctorFee = invoice.DoctorFee,
                Total = invoice.Total,

                Items = invoice.Items.Select(i => new InvoiceItemDto
                {
                    CatalogItemId = i.CatalogItemId,
                    Quantity = i.Quantity,

                }).ToList()
            };
        }

    }
}