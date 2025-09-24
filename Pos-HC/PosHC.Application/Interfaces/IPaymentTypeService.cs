using PosHC.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PosHC.Application.Interfaces
{
    public interface IPaymentTypeService
    {
        Task<List<PaymentTypeLookupDto>> GetPaymentTypeLookupDtos(CancellationToken cancellationToken = default);
    }
}
