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
    }
}
