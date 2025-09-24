using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PosHC.Domain.Entities
{
    public class PaymentType
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }

}
