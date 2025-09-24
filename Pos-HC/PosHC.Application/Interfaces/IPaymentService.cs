using PosHC.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PosHC.Application.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentResultDto> SavePayment(PaymentRequestDto paymentRequestDto,CancellationToken cancellationToken = default);
    }
}
