using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TurnosMedicos.Data;
using TurnosMedicos.DTOs;
using TurnosMedicos.Helpers;
using TurnosMedicos.Models;

namespace TurnosMedicos.Controllers;

[ApiController]
[Route("[controller]")]
public class TurnosController : ControllerBase
{
    private readonly AppDbContext _context;

    public TurnosController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var turnos = await _context.Turnos
            .Include(t => t.Paciente)
            .Include(t => t.Medico)
            .ToListAsync();
        return Ok(turnos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var turno = await _context.Turnos
            .Include(t => t.Paciente)
            .Include(t => t.Medico)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (turno == null) 
            return NotFound("Turno no encontrado.");

        return Ok(turno);
    }

    [HttpPost]
    public async Task<IActionResult> CrearTurno([FromBody] TurnoDTO turno)
    {
        var paciente = await _context.Pacientes.FindAsync(turno.PacienteId);

        if (paciente == null)
            return NotFound(new { mensaje = "Paciente no encontrado." });


        if (paciente.Bloqueado)
        {
            var fechaBloqueoFin = paciente.FechaBloqueo?.AddDays(30);
            if (fechaBloqueoFin >= DateTime.Now)
                return BadRequest(new { mensaje = "El paciente se encuentra bloqueado para agendar turnos online." });
            else
                await DesbloquearUsuario(paciente);
        }
            

        if(!paciente.isActive)
            return BadRequest(new { mensaje = "El paciente no está activo para agendar turnos online." });

        var medicoExiste = await _context.Medicos.AnyAsync(m => m.Id == turno.MedicoId);
        if (!medicoExiste)
            return NotFound(new { mensaje = "Médico no encontrado." });

        var turnoConflicto = await _context.Turnos.AnyAsync(t =>
            t.MedicoId == turno.MedicoId &&
            t.FechaHora == turno.FechaHora &&
            t.Estado != EstadoTurno.Cancelado);

        if (turnoConflicto)
            return BadRequest(new { mensaje = "El médico ya tiene un turno en ese horario." });

        var nuevoTurno = new Turno
        {
            PacienteId = turno.PacienteId,
            MedicoId = turno.MedicoId,
            FechaHora = turno.FechaHora,
            Motivo = turno.Motivo,
            FechaCreacion = DateTime.UtcNow,
            Estado = EstadoTurno.Pendiente
        };

        _context.Turnos.Add(nuevoTurno);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = nuevoTurno.Id }, nuevoTurno);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> ActualizarEstado(int id, [FromBody] ActualizarEstadoRequest request)
    {
        var turno = await _context.Turnos.FindAsync(id);
        if (turno == null) return NotFound();

        turno.Estado = request.Estado;

        //ausente
        if (turno.Estado == EstadoTurno.NoShow)
        {
            if (!turno.FechaHora.IsWithinCancellationWindow())
                return BadRequest(new { mensaje = "La ausencia solo puede registrarse dentro de las 24 horas del turno." });

            await MarcarNohow((int)turno.PacienteId!);                     
        }

        //cancelado
        if(turno.Estado == EstadoTurno.Cancelado)
        {
            if (turno.FechaHora - DateTime.Now < TimeSpan.FromHours(23))
            {
                if (turno.FechaHora - DateTime.Now < TimeSpan.FromHours(23))
                {
                    await MarcarNohow((int)turno.PacienteId!);
                }
            }
        }

        await _context.SaveChangesAsync();
        return Ok(turno);
    }

    private async Task MarcarNohow(int pacienteId)
    {
        var paciente = await _context.Pacientes.Where(x => x.Id == pacienteId).FirstOrDefaultAsync();

        paciente!.NoShowCount++;

        if(paciente.NoShowCount == 3)
        {
            paciente.Bloqueado = true;
            paciente.FechaBloqueo = DateTime.Now;
            paciente.NoShowCount = 0;
        }

        await _context.SaveChangesAsync();
    }

    private async Task DesbloquearUsuario(Paciente paciente)
    {
        paciente.Bloqueado = false;
        paciente.FechaBloqueo = null;

        await _context.SaveChangesAsync();
    }

}

public class ActualizarEstadoRequest
{
    public EstadoTurno Estado { get; set; }
}
