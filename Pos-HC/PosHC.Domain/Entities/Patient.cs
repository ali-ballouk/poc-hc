using System.ComponentModel.DataAnnotations.Schema;

namespace PosHC.Domain.Entities
{
    [Table("Patient", Schema = "poshc")]
    public class Patient
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } 
        public string LastName { get; set; }
    }
}
