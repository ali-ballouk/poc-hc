using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;
using PosHC.Domain.Entities;
using System;
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

        public async Task<List<Payment>> GetAllPayemtsAsync(CancellationToken cancellationToken = default)
        {
            var payments = await _poshsRepository.GetAllPaymentAsync(cancellationToken);
            return payments;
        }

        public async Task<Payment?> GetPaymentByInvoiceIdAsync(Guid invoiceId, CancellationToken cancellationToken = default)
        {
            var payments = await GetAllPayemtsAsync();
            var payment = payments.FirstOrDefault(p => p.InvoiceId == invoiceId);
            return payment; 
        }

        public async Task<PaymentResultDto> SavePayment(PaymentRequestDto paymentRequestDto, CancellationToken cancellationToken = default)
        {
            var otherInvoice = await GetPaymentByInvoiceIdAsync(paymentRequestDto.InvoiceId);
            if (otherInvoice != null)
                throw new Exception("Payment for this invoice already exists.");
            if (paymentRequestDto.Settings == null)
            {
                throw new Exception($"Payment method '{paymentRequestDto.Settings}' requires non-empty settings.");
            }

            switch (paymentRequestDto.Settings)
            {
                case CashPaymentSettings cash:
                    if (string.IsNullOrWhiteSpace(cash.CashDrawerId))
                        throw new Exception("Cash payments require a valid cash drawer ID.");
                    break;

                case CardPaymentSettings card:
                    if (string.IsNullOrWhiteSpace(card.CardNumber) || string.IsNullOrWhiteSpace(card.Expiry))
                        throw new Exception("Card payments require both card number and expiry date.");
                    break;

                case OnAccountPaymentSettings account:
                    if (string.IsNullOrWhiteSpace(account.AccountId))
                        throw new Exception("On-account payments require an account number.");
                    break;

                default:
                    throw new Exception($"Unknown payment method '{paymentRequestDto.PaymentTypeId}'.");
            }

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