using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VestindoSonhos.Models;
using VestindoSonhos.Models.DTOs;

namespace VestindoSonhos.Controllers.Api
{
    /// <summary>
    /// Controlador REST para gerenciamento e consulta de eventos da ONG Vestindo Sonhos.
    /// Exige autenticação JWT válida para acesso seguro pelo aplicativo mobile e integrações.
    /// </summary>
    [ApiController]
    [Route("api/eventos")]
    [Authorize]
    public class EventosApiController : ControllerBase
    {
        private readonly VestindosonhosdbContext _context;
        private readonly ILogger<EventosApiController> _logger;

        public EventosApiController(
            VestindosonhosdbContext context,
            ILogger<EventosApiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Obtém a lista de eventos ativos (cuja data de término é igual ou superior ao momento atual).
        /// </summary>
        /// <returns>Lista de eventos ativos formatados em JSON</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<EventoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<EventoDto>>> ObterEventosAtivos()
        {
            var agora = DateTime.Now;

            // Busca eventos ativos ordenados por data de início
            var eventos = await _context.Evento
                .AsNoTracking()
                .Include(e => e.ResponsavelNavigation)
                .Include(e => e.VoluntarioAtribuido)
                .Where(e => e.data_fim == null || e.data_fim >= agora)
                .OrderBy(e => e.data_inicio)
                .Select(e => new EventoDto
                {
                    Id = e.Id,
                    Nome = e.Nome,
                    DataInicio = e.data_inicio,
                    DataFim = e.data_fim,
                    Local = e.Local,
                    Observacao = e.Observacao,
                    ResponsavelId = e.Responsavel,
                    ResponsavelNome = e.ResponsavelNavigation != null ? e.ResponsavelNavigation.Nome : string.Empty,
                    TotalVoluntarios = e.VoluntarioAtribuido.Count
                })
                .ToListAsync();

            _logger.LogInformation("Consulta de eventos ativos executada com sucesso. Total retornado: {Total}", eventos.Count);

            return Ok(eventos);
        }

        /// <summary>
        /// Obtém os detalhes de um evento específico pelo seu identificador (ID).
        /// </summary>
        /// <param name="id">Identificador único do evento</param>
        /// <returns>Detalhes do evento solicitado</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(EventoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<EventoDto>> ObterPorId(int id)
        {
            var evento = await _context.Evento
                .AsNoTracking()
                .Include(e => e.ResponsavelNavigation)
                .Include(e => e.VoluntarioAtribuido)
                .Where(e => e.Id == id)
                .Select(e => new EventoDto
                {
                    Id = e.Id,
                    Nome = e.Nome,
                    DataInicio = e.data_inicio,
                    DataFim = e.data_fim,
                    Local = e.Local,
                    Observacao = e.Observacao,
                    ResponsavelId = e.Responsavel,
                    ResponsavelNome = e.ResponsavelNavigation != null ? e.ResponsavelNavigation.Nome : string.Empty,
                    TotalVoluntarios = e.VoluntarioAtribuido.Count
                })
                .FirstOrDefaultAsync();

            if (evento == null)
            {
                return NotFound(new { mensagem = $"Evento com identificador {id} não foi encontrado." });
            }

            return Ok(evento);
        }

        [HttpPost]
        public async Task<ActionResult<EventoDto>> Criar([FromBody] CriarEditarEventoDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nome))
            {
                return BadRequest(new { mensagem = "O nome do evento é obrigatório." });
            }

            var idUsuarioClaim = User.FindFirst("UserId")?.Value 
                ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            int idUsuario = 1;
            if (!string.IsNullOrEmpty(idUsuarioClaim) && int.TryParse(idUsuarioClaim, out int parsedId))
            {
                idUsuario = parsedId;
            }

            var nome = dto.Nome.Trim();
            if (nome.Length > 50) nome = nome.Substring(0, 50);

            var local = dto.Local?.Trim();
            if (local != null && local.Length > 70) local = local.Substring(0, 70);

            var obs = dto.Observacao?.Trim();
            if (obs != null && obs.Length > 255) obs = obs.Substring(0, 255);

            var evento = new Evento
            {
                Nome = nome,
                data_inicio = dto.DataInicio ?? DateTime.Now,
                data_fim = dto.DataFim ?? DateTime.Now.AddHours(2),
                Local = local,
                Observacao = obs,
                Responsavel = idUsuario
            };

            _context.Evento.Add(evento);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(ObterPorId), new { id = evento.Id }, new EventoDto
            {
                Id = evento.Id,
                Nome = evento.Nome,
                DataInicio = evento.data_inicio,
                DataFim = evento.data_fim,
                Local = evento.Local,
                Observacao = evento.Observacao,
                ResponsavelId = evento.Responsavel,
                TotalVoluntarios = 0
            });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Editar(int id, [FromBody] CriarEditarEventoDto dto)
        {
            var evento = await _context.Evento.FindAsync(id);
            if (evento == null)
            {
                return NotFound(new { mensagem = "Evento não encontrado." });
            }

            if (string.IsNullOrWhiteSpace(dto.Nome))
            {
                return BadRequest(new { mensagem = "O nome do evento é obrigatório." });
            }

            var nome = dto.Nome.Trim();
            if (nome.Length > 50) nome = nome.Substring(0, 50);

            var local = dto.Local?.Trim();
            if (local != null && local.Length > 70) local = local.Substring(0, 70);

            var obs = dto.Observacao?.Trim();
            if (obs != null && obs.Length > 255) obs = obs.Substring(0, 255);

            evento.Nome = nome;
            evento.data_inicio = dto.DataInicio ?? evento.data_inicio;
            evento.data_fim = dto.DataFim ?? evento.data_fim;
            evento.Local = local;
            evento.Observacao = obs;

            _context.Evento.Update(evento);
            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Evento atualizado com sucesso." });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Excluir(int id)
        {
            var evento = await _context.Evento.FindAsync(id);
            if (evento == null)
            {
                return NotFound(new { mensagem = "Evento não encontrado." });
            }

            var vols = await _context.VoluntarioAtribuido.Where(va => va.Evento == id).ToListAsync();
            if (vols.Any())
            {
                _context.VoluntarioAtribuido.RemoveRange(vols);
                await _context.SaveChangesAsync();
            }

            _context.Evento.Remove(evento);
            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Evento excluído com sucesso." });
        }
    }
}
