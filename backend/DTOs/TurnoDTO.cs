using System.ComponentModel.DataAnnotations;

namespace TurnosMedicos.DTOs
{
    public class TurnoDTO
    {
        [Required(ErrorMessage = "El Id de paciente es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "Paciente Id invalido")]
        public int PacienteId { get; set; }

        [Required(ErrorMessage = "El Id del medico es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "Medico Id inválido")]
        public int MedicoId { get; set; }

        [Required(ErrorMessage = "La fecha y hora son obligatorias")]
        public DateTime FechaHora { get; set; }

        [Required(ErrorMessage = "El motivo es obligatorio")]
        [StringLength(200, ErrorMessage = "El motivo no puede superar los 200 caracteres")]
        [MinLength(5, ErrorMessage = "El motivo debe tener al menos 5 caracteres")]
        public string Motivo { get; set; } = string.Empty;
    }
}
