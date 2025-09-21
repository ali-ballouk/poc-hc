using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PosHC.Application.DTOs
{
    public class PatientLookupDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
    }
}
