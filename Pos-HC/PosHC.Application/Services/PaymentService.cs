using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;
using PosHC.Domain.Entities;
using System.Numerics;
namespace PosHC.Application.Services
{
    public class PaymentService : IPaymentService
    {

        private readonly IPOSHCRepository _poshsRepository;

        public PaymentService(IPOSHCRepository poshsRepository)
        {
            _poshsRepository = poshsRepository;
        }
        public async Task<PaymentResultDto> SavePayment(PaymentRequestDto paymentRequestDto, CancellationToken cancellationToken = default)
        {
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                InvoiceId = paymentRequestDto.InvoiceId,
                PaymentTypeId = paymentRequestDto.PaymentTypeId,
                PaymentDate = DateTime.Now,
                Settings = System.Text.Json.JsonSerializer.Serialize(paymentRequestDto.Settings)

            };
            var paymentTypes = await _poshsRepository.SavePayment(payment, cancellationToken);

            return new PaymentResultDto
            {
                Id = payment.Id,
                InvoiceId = payment.InvoiceId,
                PaymentTypeId = payment.PaymentTypeId,
                PaymentDate = payment.PaymentDate
            };
        }
    }
}