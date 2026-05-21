namespace TurnosMedicos.DTOs
{
    public class PutPacienteDTO
    {
        public string NombreCompleto { get; set; } = string.Empty;
        public string DNI { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
