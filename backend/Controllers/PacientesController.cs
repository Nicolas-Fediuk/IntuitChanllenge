using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TurnosMedicos.Data;
using TurnosMedicos.DTOs;
using TurnosMedicos.Models;

namespace TurnosMedicos.Controllers;

[ApiController]
[Route("[controller]")]
public class PacientesController : ControllerBase
{
    private readonly AppDbContext _context;

    public PacientesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var pacientes = await _context.Pacientes.ToListAsync();
        return Ok(pacientes);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var paciente = await _context.Pacientes.FindAsync(id);
        if (paciente == null) return NotFound();
        return Ok(paciente);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PostPacienteDTO paciente)
    {
        var newPaciente = new Paciente
        {
            NombreCompleto = paciente.NombreCompleto,
            DNI = paciente.DNI,
            Email = paciente.Email,
            Telefono = paciente.Telefono,
            NoShowCount = 0,
            Bloqueado = false,
            FechaBloqueo = null,
            createdAt = DateTime.Now,
            isActive = true
        };

        _context.Pacientes.Add(newPaciente);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = newPaciente.Id }, newPaciente);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] PutPacienteDTO paciente)
    {
        var existing = await _context.Pacientes.FindAsync(id);

        if (existing == null) 
            return NotFound("Paciente no encontrado");

        var putPaciente = new Paciente
        {
            NombreCompleto = paciente.NombreCompleto,
            DNI = paciente.DNI,
            Email = paciente.Email,
            Telefono = paciente.Telefono,
            isActive = paciente.IsActive
        };

        existing.NombreCompleto = putPaciente.NombreCompleto;
        existing.DNI = putPaciente.DNI;
        existing.Email = putPaciente.Email;
        existing.Telefono = putPaciente.Telefono;
        existing.isActive = putPaciente.isActive;

        await _context.SaveChangesAsync();
        return Ok(existing);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var paciente = await _context.Pacientes.FindAsync(id);
        if (paciente == null) return NotFound();

        _context.Pacientes.Remove(paciente);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
