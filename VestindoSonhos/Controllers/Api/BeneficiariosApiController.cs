using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VestindoSonhos.Models;
using VestindoSonhos.Models.DTOs;

namespace VestindoSonhos.Controllers.Api
{
    [ApiController]
    [Route("api/beneficiarios")]
    [Authorize]
    public class BeneficiariosApiController : ControllerBase
    {
        private readonly VestindosonhosdbContext _context;

        public BeneficiariosApiController(VestindosonhosdbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BeneficiarioDto>>> ObterTodos()
        {
            var beneficiarios = await _context.Beneficiario
                .AsNoTracking()
                .Include(b => b.Dependente)
                .OrderBy(b => b.Nome)
                .Select(b => new BeneficiarioDto
                {
                    Id = b.Id,
                    Nome = b.Nome,
                    Telefone = b.Telefone,
                    Endereco = b.Endereco,
                    Situacao = b.Situacao ?? "Ok",
                    Observacao = b.Observacao,
                    TotalDependentes = b.Dependente.Count
                })
                .ToListAsync();

            return Ok(beneficiarios);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<BeneficiarioDto>> ObterPorId(int id)
        {
            var b = await _context.Beneficiario
                .AsNoTracking()
                .Include(x => x.Dependente)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (b == null)
            {
                return NotFound(new { mensagem = "Beneficiário não encontrado." });
            }

            return Ok(new BeneficiarioDto
            {
                Id = b.Id,
                Nome = b.Nome,
                Telefone = b.Telefone,
                Endereco = b.Endereco,
                Situacao = b.Situacao ?? "Ok",
                Observacao = b.Observacao,
                TotalDependentes = b.Dependente.Count
            });
        }

        [HttpPost]
        public async Task<ActionResult<BeneficiarioDto>> Criar([FromBody] CriarEditarBeneficiarioDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nome))
            {
                return BadRequest(new { mensagem = "O nome do beneficiário é obrigatório." });
            }

            var telefone = System.Text.RegularExpressions.Regex.Replace(dto.Telefone ?? "", @"\D", "");
            if (telefone.Length > 11) telefone = telefone.Substring(0, 11);

            var nome = dto.Nome.Trim();
            if (nome.Length > 80) nome = nome.Substring(0, 80);

            var endereco = (dto.Endereco ?? "").Trim();
            if (endereco.Length > 60) endereco = endereco.Substring(0, 60);

            var observacao = dto.Observacao?.Trim();
            if (observacao != null && observacao.Length > 255) observacao = observacao.Substring(0, 255);

            var situacao = string.IsNullOrWhiteSpace(dto.Situacao) ? "Ok" : dto.Situacao.Trim();
            if (situacao.Length > 20) situacao = situacao.Substring(0, 20);

            var beneficiario = new Beneficiario
            {
                Nome = nome,
                Telefone = telefone,
                Endereco = endereco,
                Situacao = situacao,
                Observacao = observacao
            };

            _context.Beneficiario.Add(beneficiario);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(ObterPorId), new { id = beneficiario.Id }, new BeneficiarioDto
            {
                Id = beneficiario.Id,
                Nome = beneficiario.Nome,
                Telefone = beneficiario.Telefone,
                Endereco = beneficiario.Endereco,
                Situacao = beneficiario.Situacao,
                Observacao = beneficiario.Observacao,
                TotalDependentes = 0
            });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Editar(int id, [FromBody] CriarEditarBeneficiarioDto dto)
        {
            var b = await _context.Beneficiario.FindAsync(id);
            if (b == null)
            {
                return NotFound(new { mensagem = "Beneficiário não encontrado." });
            }

            if (string.IsNullOrWhiteSpace(dto.Nome))
            {
                return BadRequest(new { mensagem = "O nome do beneficiário é obrigatório." });
            }

            var telefone = System.Text.RegularExpressions.Regex.Replace(dto.Telefone ?? "", @"\D", "");
            if (telefone.Length > 11) telefone = telefone.Substring(0, 11);

            var nome = dto.Nome.Trim();
            if (nome.Length > 80) nome = nome.Substring(0, 80);

            var endereco = (dto.Endereco ?? "").Trim();
            if (endereco.Length > 60) endereco = endereco.Substring(0, 60);

            var observacao = dto.Observacao?.Trim();
            if (observacao != null && observacao.Length > 255) observacao = observacao.Substring(0, 255);

            var situacao = string.IsNullOrWhiteSpace(dto.Situacao) ? "Ok" : dto.Situacao.Trim();
            if (situacao.Length > 20) situacao = situacao.Substring(0, 20);

            b.Nome = nome;
            b.Telefone = telefone;
            b.Endereco = endereco;
            b.Situacao = situacao;
            b.Observacao = observacao;

            _context.Beneficiario.Update(b);
            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Beneficiário atualizado com sucesso." });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Excluir(int id)
        {
            var b = await _context.Beneficiario.FindAsync(id);
            if (b == null)
            {
                return NotFound(new { mensagem = "Beneficiário não encontrado." });
            }

            try
            {
                var deps = await _context.Dependente.Where(d => d.IdResponsavel == id).ToListAsync();
                if (deps.Any())
                {
                    _context.Dependente.RemoveRange(deps);
                }

                var transacoesInv = await _context.TransacaoInventario.Where(ti => ti.Beneficiario == id).ToListAsync();
                foreach (var ti in transacoesInv)
                {
                    ti.Beneficiario = null;
                }

                await _context.SaveChangesAsync();

                _context.Beneficiario.Remove(b);
                await _context.SaveChangesAsync();

                return Ok(new { mensagem = "Beneficiário excluído com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = $"Erro ao excluir beneficiário: {ex.Message}" });
            }
        }
    }
}
