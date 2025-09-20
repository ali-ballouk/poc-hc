using System.ComponentModel.DataAnnotations.Schema;

namespace PosHC.Domain.Entities
{
    [Table("Doctor", Schema = "poshc")]
    public class Doctor
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } 
        public string LastName { get; set; }
    }
}
