using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VestindoSonhos.Models;
using VestindoSonhos.ViewModels;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Diagnostics;

namespace VestindoSonhos.Controllers
{
    
    public class FinanceiroController : Controller
    {

        private readonly VestindosonhosdbContext _context;

        public FinanceiroController(VestindosonhosdbContext context)
        {
            _context = context;
        }

        // GET: Financeiro
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Financeiro()
        {
            var resumo_financeiro = new HomeFinanceiraViewModel
            {
                lista_carteiras =  (from c in _context.Carteira
                                   select new Carteira
                                   {
                                       Saldo = c.Saldo,
                                       Nome = c.Nome,
                                       Id = c.Id
                                   }).ToList(),
                lista_transacoes = (from tf in _context.TransacaoFinanceira
                                    join t in _context.Transacao on tf.Id equals t.Id
                                    join u in _context.Usuario on t.Usuario equals u.Id
                                    join c in _context.Categoria on t.Categoria equals c.Id
                                    where c.Id != 1
                                    orderby t.Hora descending
                                    select new TransacaoFinanceiraViewModel
                                    {
                                        Id = t.Id,
                                        Valor = tf.Valor,
                                        UsuarioId = u.Id,
                                        Usuario = u.Nome,
                                        Razao = t.Razao.ToString(),
                                        Hora = t.Hora,
                                        Categoria = c.Id,
                                        Observacao = t.Observacao,
                                        CategoriaNavigation = t.CategoriaNavigation
                                    }).Take(15).ToList(),
                lista_doacoes = (from tf in _context.TransacaoFinanceira
                                 join t in _context.Transacao on tf.Id equals t.Id
                                 join u in _context.Usuario on t.Usuario equals u.Id
                                 join c in _context.Categoria on t.Categoria equals c.Id
                                 where t.Categoria == 1
                                 orderby t.Hora descending
                                 select new TransacaoFinanceiraViewModel
                                 {
                                     Id = t.Id,
                                     Valor = tf.Valor,
                                     UsuarioId = u.Id,
                                     Usuario = u.Nome,
                                     Razao = t.Razao.ToString(),
                                     Hora = t.Hora,
                                     Categoria = c.Id,
                                     Observacao = t.Observacao,
                                     CategoriaNavigation = t.CategoriaNavigation
                                 }).Take(15).ToList()
            };
            return View(resumo_financeiro);
        }

        // GET: Financeiro/Resumo
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Resumo()
        {

            var transacoes = (from tf in _context.TransacaoFinanceira
                              join t in _context.Transacao on tf.Id equals t.Id
                              join u in _context.Usuario on t.Usuario equals u.Id
                              join c in _context.Categoria on t.Categoria equals c.Id
                              where c.Id != 1
                              orderby t.Hora descending
                              select new TransacaoFinanceiraViewModel
                              {
                                  Valor = tf.Valor,
                                  UsuarioId = u.Id,
                                  Usuario = u.Nome,
                                  Razao = t.Razao.ToString(),
                                  Hora = t.Hora,
                                  Categoria = c.Id,
                                  Observacao = t.Observacao
                              }).ToList();
            return View(transacoes);
        }

        // GET: Financeiro/Doacoes
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Doacoes()
        {

            var transacoes = (from tf in _context.TransacaoFinanceira
                              join t in _context.Transacao on tf.Id equals t.Id
                              join u in _context.Usuario on t.Usuario equals u.Id
                              join c in _context.Categoria on t.Categoria equals c.Id
                              where t.Categoria == 1
                              orderby t.Hora descending
                              select new TransacaoFinanceiraViewModel
                              {
                                  Valor = tf.Valor,
                                  UsuarioId = u.Id,
                                  Usuario = u.Nome,
                                  Razao = t.Razao.ToString(),
                                  Hora = t.Hora,
                                  Categoria = c.Id,
                                  Observacao = t.Observacao
                              }).ToList();
            return View(transacoes);
        }

        // GET: Financeiro/Dashboard
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Dashboard()
        {
            var contexto = new DashboardViewModel
            {
                estatisticasFinanceiras = await (from tf in _context.TransacaoFinanceira
                                                 join t in _context.Transacao on tf.Id equals t.Id
                                                 where t.CategoriaNavigation.Nome != "Doação"
                                                 select new DashboardViewModel.EstatisticasFinanceiras
                                                 {
                                                     Data = t.Hora,
                                                     Razao = t.Razao,
                                                     Tipo = t.CategoriaNavigation.Nome,
                                                     Valor = tf.Valor
                                                 }).ToListAsync(),
                estatisticasFinanceirasDoacao = await (from tf in _context.TransacaoFinanceira
                                                       join t in _context.Transacao on tf.Id equals t.Id
                                                       where t.CategoriaNavigation.Nome == "Doação"
                                                       select new DashboardViewModel.EstatisticasFinanceirasDoacao
                                                       {
                                                           Data = t.Hora,
                                                           Valor = tf.Valor
                                                       }).ToListAsync(),
                estatisticasRoupas = await (from ti in _context.TransacaoInventario
                                            join t in _context.Transacao on ti.TransacaoId equals t.Id
                                            where t.Razao == "Saída"
                                            select new DashboardViewModel.EstatisticasDoacoesSaida
                                            {
                                                Quantidade = ti.Quantidade,
                                                Data = t.Hora,
                                            }
                                            ).OrderByDescending(t => t.Data).ToListAsync(),
                EventosAgendados = await (from e in _context.Evento
                                          where e.data_fim >= DateTime.Now
                                          select e
                                          ).CountAsync(),
                TotalVoluntarios = await _context.Voluntario.CountAsync(),
                estatisticasVoluntariosAtribuidos = await (from e in _context.Evento
                                                           join va in _context.VoluntarioAtribuido on e.Id equals va.Evento
                                                           join v in _context.Voluntario on va.Voluntario equals v.Id
                                                           where e.data_fim >= DateTime.Now
                                                           select new DashboardViewModel.EstatisticasVoluntariosAtribuidos
                                                           {
                                                               nomeEvento = e.Nome,
                                                               voluntariosAtribuidos = _context.VoluntarioAtribuido.Where(va => e.Id == va.Evento).Count(),
                                                           }
                                                    ).Distinct().ToListAsync()
            };
            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize(contexto, options);
            System.IO.File.WriteAllText(".\\DataAnalysis\\data.json", json);
            GerarGraficosDashboard();
            return View();
        }

        // GET: Financeiro/Criar
        [Authorize(Roles = "Administrador")]
        public IActionResult Criar(string origem)
        {
            ViewBag.Origem = origem;
            CarregarDropdowns();
            return View();
        }

        private void CarregarDropdowns()
        {
            ViewData["Categoria"] = new SelectList(_context.Categoria, "Id", "Nome");
            ViewData["Carteira"] = new SelectList(_context.Carteira, "Id", "Nome");
            ViewBag.Razao = new SelectList(new[] { "Entrada", "Saída" });
        }

        // POST: Financeiro/Criar
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar([Bind("Id,Hora,Observacao,Categoria,Razao,Valor,Carteira")] TransacaoFinanceiraViewModel tfViewModel)
        {
            //FOREACH para DEBUG
            foreach (var erro in ModelState)
            {
                foreach (var subErro in erro.Value.Errors)
                {
                    Console.WriteLine($"{erro.Key}: {subErro.ErrorMessage}");
                }
            }
            CarregarDropdowns();
            if (ModelState.IsValid)
            {
                Transacao.TipoTransacao razaoEnum;

                try
                {
                    razaoEnum = Enum.Parse<Transacao.TipoTransacao>(tfViewModel.Razao);
                }
                catch
                {
                    ModelState.AddModelError(nameof(tfViewModel.Razao), "Razão inválida.");
                    CarregarDropdowns();
                    return View(tfViewModel);
                }

                tfViewModel.Valor = decimal.Parse(Request.Form["Valor"].ToString().Replace(".", ","));

                if (tfViewModel.Valor <= 0)
                {
                    CarregarDropdowns();
                    return View(tfViewModel);
                }
                
                var categoriaId = tfViewModel.Categoria;
                var categoriaEntity = _context.Categoria.FirstOrDefault(c => c.Id == categoriaId);
                if (categoriaEntity == null)
                {
                    ModelState.AddModelError(nameof(tfViewModel.Categoria), "Categoria inválida.");
                    CarregarDropdowns();
                    return View(tfViewModel);
                }
                var idUsuarioClaim = User.FindFirst("UserId")?.Value 
                    ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                int idUsuario = 1;
                if (!string.IsNullOrEmpty(idUsuarioClaim) && int.TryParse(idUsuarioClaim, out int parsedId))
                {
                    idUsuario = parsedId;
                }

                var transacao = new Transacao
                {
                    Hora = tfViewModel.Hora,
                    Observacao = tfViewModel.Observacao,
                    Razao = tfViewModel.Razao,
                    Categoria = categoriaEntity.Id,
                    Usuario = idUsuario
                };

                var transacaoFinanceira = new TransacaoFinanceira
                {
                    Valor = tfViewModel.Valor,
                    Carteira = tfViewModel.Carteira,
                    IdNavigation = transacao
                };

                // Atualizar Carteira com segurança
                var carteira = _context.Carteira.FirstOrDefault(c => c.Id == tfViewModel.Carteira);
                if (carteira != null)
                {
                    if (razaoEnum == Transacao.TipoTransacao.Entrada) 
                    {
                        carteira.Saldo += tfViewModel.Valor;
                    }
                    else if (razaoEnum == Transacao.TipoTransacao.Saída)
                    {
                        carteira.Saldo -= tfViewModel.Valor;
                    }
                    _context.Carteira.Update(carteira);
                }

                _context.Add(transacaoFinanceira);
                _context.Add(transacao);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Financeiro));
            }

            CarregarDropdowns();
            return View(tfViewModel);
        }

        // GET: Financeiro/Editar/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Console.WriteLine("Editando");
            var transacao = (from tf in _context.TransacaoFinanceira
                             join t in _context.Transacao on tf.Id equals t.Id
                             join u in _context.Usuario on t.Usuario equals u.Id
                             join c in _context.Categoria on t.Categoria equals c.Id
                             join cart in _context.Carteira on tf.Carteira equals cart.Id
                             where tf.Id == id
                             select new TransacaoFinanceiraEditViewModel
                             {
                                 Valor = tf.Valor,
                                 UsuarioId = u.Id,
                                 Usuario = u.Nome,
                                 Razao = t.Razao.ToString(),
                                 Hora = t.Hora,
                                 Categoria = c.Id,
                                 Observacao = t.Observacao,
                                 Carteira = cart.Id,

                             }).FirstOrDefault();
            ViewData["Carteira"] = new SelectList(_context.Carteira, "Id", "Nome");

            if (transacao == null)
            {
                return NotFound();
            }
            CarregarDropdowns();
            return View(transacao);
        }

        // GET: Financeiro/Apagar/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Apagar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transacao = await _context.Transacao
                .Include(t => t.UsuarioNavigation).Include(t => t.CategoriaNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (transacao == null)
            {
                return NotFound();
            }

            return View(transacao);
        }

        // POST: Financeiro/Apagar/5
        [Authorize(Roles = "Administrador")]
        [HttpPost, ActionName("Apagar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarExclusão(int id)
        {
            var transacao_financeira = await _context.TransacaoFinanceira.FindAsync(id);
            if (transacao_financeira != null)
            {
                _context.TransacaoFinanceira.Remove(transacao_financeira);
                await _context.SaveChangesAsync();
            }

            var transacao = await _context.Transacao.FindAsync(id);
            if (transacao != null)
            {
                _context.Transacao.Remove(transacao);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Financeiro));
        }

        private bool TransacaoExists(int id)
        {
            return _context.Transacao.Any(e => e.Id == id);
        }

        // POST: Financeiro/Editar/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, [Bind("Id,Hora,UsuarioId,Observacao,Categoria,Razao,Valor,Carteira")] TransacaoFinanceiraEditViewModel tfViewModel)
        {
            if (!_context.TransacaoFinanceira.Any(tf => tf.Id == id))
            {
                return NotFound();
            }
            //FOREACH para DEBUG
            foreach (var erro in ModelState)
            {
                foreach (var subErro in erro.Value.Errors)
                {
                    Console.WriteLine($"{erro.Key}: {subErro.ErrorMessage}");
                }
            }
            if (ModelState.IsValid)
            {
                var transacaoFinanceira = _context.TransacaoFinanceira.FirstOrDefault(tf => tf.Id == id);
                var transacao = _context.Transacao.FirstOrDefault(t => t.Id == id);

                tfViewModel.Valor = decimal.Parse(Request.Form["Valor"].ToString().Replace(".", ","));

                if (tfViewModel.Valor <= 0)
                {
                    CarregarDropdowns();
                    return View(tfViewModel);
                }

                var categoriaId = tfViewModel.Categoria;
                var categoriaEntity = _context.Categoria.FirstOrDefault(c => c.Id == categoriaId);
                if (categoriaEntity == null)
                {
                    ModelState.AddModelError(nameof(tfViewModel.Categoria), "Categoria inválida.");
                    CarregarDropdowns();
                    return View(tfViewModel);
                }

                // Preparar updates no banco
                transacao.Hora = tfViewModel.Hora;
                transacao.Observacao = tfViewModel.Observacao;
                transacao.Categoria = categoriaEntity.Id;
                transacao.Usuario = tfViewModel.UsuarioId;

                decimal valorAntigo = transacaoFinanceira.Valor;
                transacaoFinanceira.Valor = tfViewModel.Valor;
                transacaoFinanceira.Carteira = tfViewModel.Carteira;
                transacaoFinanceira.IdNavigation = transacao;

                // Atualizar Carteira com segurança
                var carteira = _context.Carteira.FirstOrDefault(c => c.Id == tfViewModel.Carteira);
                if (carteira != null)
                {
                    if (valorAntigo < tfViewModel.Valor)
                    {
                        carteira.Saldo += (tfViewModel.Valor - valorAntigo);
                    }
                    else
                    {
                        carteira.Saldo -= (valorAntigo - tfViewModel.Valor);
                    }
                    _context.Carteira.Update(carteira);
                }

                _context.Update(transacao);
                _context.Update(transacaoFinanceira);
                await _context.SaveChangesAsync();
                
                return RedirectToAction(nameof(Financeiro));
            }
            //ViewData["Usuario"] = new SelectList(_context.Usuario, "Id", "Nome", tfViewModel.Usuario);
            return View(tfViewModel);
        }

        private void GerarGraficosDashboard()
        {
            var pythonPath = @".\DataAnalysis\.venv\Scripts\python.exe";
            if (!System.IO.File.Exists(pythonPath))
            {
                return;
            }

            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = pythonPath,
                    Arguments = @".\DataAnalysis\Scriptdata.py",
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(processInfo))
                {
                    if (process != null)
                    {
                        string output = process.StandardOutput.ReadToEnd();
                        string erro = process.StandardError.ReadToEnd();
                        process.WaitForExit();
                        Console.WriteLine(output);
                        Console.WriteLine(erro);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao gerar gráfico: {ex.Message}");
            }
        }
    }
}