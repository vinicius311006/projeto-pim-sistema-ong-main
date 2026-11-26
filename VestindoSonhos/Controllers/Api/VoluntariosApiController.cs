using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using VestindoSonhos.Helpers;
using VestindoSonhos.Models;

namespace VestindoSonhos.Controllers.Api
{
    [ApiController]
    [Route("api/voluntarios")]
    [Authorize(Roles = "Administrador,Colaborador")]
    public class VoluntariosApiController : ControllerBase
    {
        private readonly VestindosonhosdbContext _context;

        public VoluntariosApiController(VestindosonhosdbContext context)
        {
            _context = context;
        }

        public class VoluntarioDto
        {
            public int Id { get; set; }
            public string Nome { get; set; } = string.Empty;
            public string? Cpf { get; set; }
            public string? Endereco { get; set; }
            public string? Telefone { get; set; }
            public string? Email { get; set; }
            public int TotalEventos { get; set; }
        }

        public class CriarEditarVoluntarioDto
        {
            public string Nome { get; set; } = string.Empty;
            public string? Cpf { get; set; }
            public string? Endereco { get; set; }
            public string? Telefone { get; set; }
            public string? Email { get; set; }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<VoluntarioDto>>> ObterTodos()
        {
            var lista = await _context.Voluntario
                .AsNoTracking()
                .Include(v => v.VoluntarioAtribuido)
                .OrderBy(v => v.Nome)
                .Select(v => new VoluntarioDto
                {
                    Id = v.Id,
                    Nome = v.Nome ?? string.Empty,
                    Cpf = v.Cpf,
                    Endereco = v.Endereco,
                    Telefone = v.Telefone,
                    Email = v.Email,
                    TotalEventos = v.VoluntarioAtribuido.Count
                })
                .ToListAsync();

            return Ok(lista);
        }

        [HttpPost]
        public async Task<ActionResult<VoluntarioDto>> Criar([FromBody] CriarEditarVoluntarioDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nome))
            {
                return BadRequest(new { mensagem = "O nome do voluntário é obrigatório." });
            }

            var cpf = SanitizarDigitos(dto.Cpf, 11);
            if (!ValidadorUsuarioHelper.CpfEhValido(cpf))
            {
                return BadRequest(new { mensagem = "CPF inválido. Verifique os 11 dígitos informados." });
            }

            var telefone = SanitizarDigitos(dto.Telefone, 11);
            if (!string.IsNullOrWhiteSpace(telefone) && !ValidadorUsuarioHelper.TelefoneEhValido(telefone))
            {
                return BadRequest(new { mensagem = "Informe um telefone válido completo com DDD (10 ou 11 dígitos)." });
            }

            var voluntario = new Voluntario
            {
                Nome = SanitizarTexto(dto.Nome, 60),
                Cpf = cpf,
                Telefone = string.IsNullOrWhiteSpace(telefone) ? null : telefone,
                Endereco = SanitizarTexto(dto.Endereco, 50),
                Email = SanitizarTexto(dto.Email, 40)
            };

            _context.Voluntario.Add(voluntario);
            await _context.SaveChangesAsync();

            return Ok(new VoluntarioDto
            {
                Id = voluntario.Id,
                Nome = voluntario.Nome ?? string.Empty,
                Cpf = voluntario.Cpf,
                Endereco = voluntario.Endereco,
                Telefone = voluntario.Telefone,
                Email = voluntario.Email,
                TotalEventos = 0
            });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Editar(int id, [FromBody] CriarEditarVoluntarioDto dto)
        {
            var voluntario = await _context.Voluntario.FindAsync(id);
            if (voluntario == null)
            {
                return NotFound(new { mensagem = "Voluntário não encontrado." });
            }

            if (string.IsNullOrWhiteSpace(dto.Nome))
            {
                return BadRequest(new { mensagem = "O nome do voluntário é obrigatório." });
            }

            var cpf = SanitizarDigitos(dto.Cpf, 11);
            if (!ValidadorUsuarioHelper.CpfEhValido(cpf))
            {
                return BadRequest(new { mensagem = "CPF inválido. Verifique os 11 dígitos informados." });
            }

            var telefone = SanitizarDigitos(dto.Telefone, 11);
            if (!string.IsNullOrWhiteSpace(telefone) && !ValidadorUsuarioHelper.TelefoneEhValido(telefone))
            {
                return BadRequest(new { mensagem = "Informe um telefone válido completo com DDD (10 ou 11 dígitos)." });
            }

            voluntario.Nome = SanitizarTexto(dto.Nome, 60);
            voluntario.Cpf = cpf;
            voluntario.Telefone = string.IsNullOrWhiteSpace(telefone) ? null : telefone;
            voluntario.Endereco = SanitizarTexto(dto.Endereco, 50);
            voluntario.Email = SanitizarTexto(dto.Email, 40);

            _context.Voluntario.Update(voluntario);
            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Voluntário atualizado com sucesso." });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Excluir(int id)
        {
            var voluntario = await _context.Voluntario.FindAsync(id);
            if (voluntario == null)
            {
                return NotFound(new { mensagem = "Voluntário não encontrado." });
            }

            var atribuicoes = await _context.VoluntarioAtribuido
                .Where(va => va.Voluntario == id)
                .ToListAsync();

            if (atribuicoes.Any())
            {
                _context.VoluntarioAtribuido.RemoveRange(atribuicoes);
                await _context.SaveChangesAsync();
            }

            _context.Voluntario.Remove(voluntario);
            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Voluntário excluído com sucesso." });
        }

        private static string SanitizarTexto(string? valor, int maxLen)
        {
            if (string.IsNullOrWhiteSpace(valor)) return string.Empty;
            var limpo = valor.Trim();
            return limpo.Length > maxLen ? limpo.Substring(0, maxLen) : limpo;
        }

        private static string SanitizarDigitos(string? valor, int maxLen)
        {
            if (string.IsNullOrWhiteSpace(valor)) return string.Empty;
            var digitos = Regex.Replace(valor, @"\D", "");
            return digitos.Length > maxLen ? digitos.Substring(0, maxLen) : digitos;
        }
    }
}
