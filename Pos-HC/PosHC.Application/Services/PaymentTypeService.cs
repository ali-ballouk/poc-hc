using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;
using PosHC.Domain.Entities;
namespace PosHC.Application.Services
{
    public class PaymentTypeService : IPaymentTypeService
    {

        private readonly IPOSHCRepository _poshsRepository;

        public PaymentTypeService(IPOSHCRepository poshsRepository)
        {
            _poshsRepository = poshsRepository;
        }

        public async Task<List<PaymentTypeLookupDto>> GetPaymentTypeLookupDtos(CancellationToken cancellationToken = default)
        {
            var types = await GetAllPaymentTypes(cancellationToken);
            return types.Select(PaymentTypeLookupDtoMapper).OrderBy(t => t.Id).ToList();
        }


        public async Task<List<PaymentType>> GetAllPaymentTypes(CancellationToken cancellationToken = default)
        {
            var paymentTypes = await _poshsRepository.GetAllPaymentTypeAsync(cancellationToken);
            return paymentTypes;
        }
        PaymentTypeLookupDto PaymentTypeLookupDtoMapper(PaymentType payment)
        {
            return new PaymentTypeLookupDto
            {
                Id = payment.Id,
                Name = payment.Name
            };
        }

    }
}