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
        [Authorize]
        public async Task<IActionResult> Dashboard()
        {
            var contexto = await ConstruirDashboardViewModelAsync();
            return View(contexto);
        }

        // GET: Financeiro/DadosDashboardJson
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> DadosDashboardJson()
        {
            var contexto = await ConstruirDashboardViewModelAsync();
            return Json(contexto);
        }

        private async Task<DashboardViewModel> ConstruirDashboardViewModelAsync()
        {
            var agora = DateTime.Now;
            var culturaPtBr = new System.Globalization.CultureInfo("pt-BR");

            // 1. KPIs Básicos (Beneficiários, Dependentes, Voluntários, Eventos)
            var totalBeneficiarios = await _context.Beneficiario.CountAsync();
            var totalDependentes = await _context.Dependente.CountAsync();
            var totalVoluntarios = await _context.Voluntario.CountAsync();
            var eventosAgendados = await _context.Evento
                .Where(e => e.data_fim == null || e.data_fim >= agora)
                .CountAsync();
            var totalEventosRealizados = await _context.Evento
                .Where(e => e.data_fim != null && e.data_fim < agora)
                .CountAsync();

            // 2. Carteiras e Transações Financeiras
            var carteiras = await _context.Carteira.AsNoTracking().ToListAsync();
            var saldoAtual = carteiras.Sum(c => c.Saldo);

            var transacoesFin = await (from tf in _context.TransacaoFinanceira.AsNoTracking()
                                       join t in _context.Transacao.AsNoTracking() on tf.Id equals t.Id
                                       join c in _context.Categoria.AsNoTracking() on t.Categoria equals c.Id into catGroup
                                       from c in catGroup.DefaultIfEmpty()
                                       join u in _context.Usuario.AsNoTracking() on t.Usuario equals u.Id into userGroup
                                       from u in userGroup.DefaultIfEmpty()
                                       join cart in _context.Carteira.AsNoTracking() on tf.Carteira equals cart.Id into cartGroup
                                       from cart in cartGroup.DefaultIfEmpty()
                                       orderby t.Hora descending
                                       select new DashboardViewModel.TransacaoRecenteItem
                                       {
                                           Id = tf.Id,
                                           DataHora = t.Hora,
                                           Razao = t.Razao ?? "Entrada",
                                           Categoria = c != null ? c.Nome : "Geral",
                                           Carteira = cart != null ? cart.Nome : "Principal",
                                           Usuario = u != null ? u.Nome : "Sistema",
                                           Observacao = t.Observacao,
                                           Valor = tf.Valor
                                       }).ToListAsync();

            var totalEntradas = transacoesFin
                .Where(x => x.Razao == "Entrada")
                .Sum(x => x.Valor);

            var totalSaidas = transacoesFin
                .Where(x => x.Razao == "Saída" || x.Razao == "Saida")
                .Sum(x => x.Valor);

            var totalDoacoes = transacoesFin
                .Where(x => x.Categoria.Equals("Doação", StringComparison.OrdinalIgnoreCase) || x.Categoria.Equals("Doacao", StringComparison.OrdinalIgnoreCase))
                .Sum(x => x.Valor);

            var transacoesMesAtual = transacoesFin
                .Where(x => x.DataHora.HasValue && x.DataHora.Value.Month == agora.Month && x.DataHora.Value.Year == agora.Year)
                .ToList();

            var entradasMesAtual = transacoesMesAtual
                .Where(x => x.Razao == "Entrada")
                .Sum(x => x.Valor);

            var saidasMesAtual = transacoesMesAtual
                .Where(x => x.Razao == "Saída" || x.Razao == "Saida")
                .Sum(x => x.Valor);

            var doacoesMesAtual = transacoesMesAtual
                .Where(x => x.Categoria.Equals("Doação", StringComparison.OrdinalIgnoreCase) || x.Categoria.Equals("Doacao", StringComparison.OrdinalIgnoreCase))
                .Sum(x => x.Valor);

            // 3. Inventário e Estoque por Categoria (TiposAtivo)
            var tiposAtivo = await _context.TiposAtivo.AsNoTracking().ToListAsync();
            var produtos = await _context.Produto
                .AsNoTracking()
                .Include(p => p.CategoriaNavigation)
                .ToListAsync();

            var movInventario = await (from ti in _context.TransacaoInventario.AsNoTracking()
                                       join t in _context.Transacao.AsNoTracking() on ti.TransacaoId equals t.Id
                                       select new
                                       {
                                           ti.ProdutoId,
                                           ti.Quantidade,
                                           t.Razao,
                                           t.Hora
                                       }).ToListAsync();

            var estoquePorProduto = produtos.Select(p =>
            {
                var ent = movInventario.Where(m => m.ProdutoId == p.Id && m.Razao == "Entrada").Sum(m => m.Quantidade);
                var sai = movInventario.Where(m => m.ProdutoId == p.Id && (m.Razao == "Saída" || m.Razao == "Saida")).Sum(m => m.Quantidade);
                return new
                {
                    Produto = p,
                    CategoriaNome = p.CategoriaNavigation?.Categoria ?? "Outros",
                    Estoque = Math.Max(0, ent - sai)
                };
            }).ToList();

            var totalProdutosCadastrados = produtos.Count;
            var totalUnidadesEstoque = estoquePorProduto.Sum(x => x.Estoque);
            var totalItensDoadosSaida = movInventario
                .Where(m => m.Razao == "Saída" || m.Razao == "Saida")
                .Sum(m => m.Quantidade);

            var inventarioPorCategoria = new List<DashboardViewModel.CategoriaQuantidadeItem>();
            if (totalUnidadesEstoque > 0)
            {
                inventarioPorCategoria = estoquePorProduto
                    .GroupBy(x => x.CategoriaNome)
                    .Select(g => new DashboardViewModel.CategoriaQuantidadeItem
                    {
                        Categoria = g.Key,
                        Quantidade = g.Sum(x => x.Estoque)
                    })
                    .OrderByDescending(x => x.Quantidade)
                    .ToList();
            }
            else if (produtos.Any())
            {
                inventarioPorCategoria = estoquePorProduto
                    .GroupBy(x => x.CategoriaNome)
                    .Select(g => new DashboardViewModel.CategoriaQuantidadeItem
                    {
                        Categoria = g.Key,
                        Quantidade = g.Count()
                    })
                    .OrderByDescending(x => x.Quantidade)
                    .ToList();
            }
            else
            {
                inventarioPorCategoria = tiposAtivo
                    .Select(ta => new DashboardViewModel.CategoriaQuantidadeItem
                    {
                        Categoria = ta.Categoria,
                        Quantidade = 0
                    })
                    .ToList();
            }

            // 4. Beneficiários por Situação e Necessidades da Comunidade
            var beneficiariosDb = await _context.Beneficiario.AsNoTracking().ToListAsync();
            var beneficiariosPorSituacao = beneficiariosDb
                .GroupBy(b => string.IsNullOrWhiteSpace(b.Situacao) ? "Ativo / Regular" : b.Situacao.Trim())
                .Select(g => new DashboardViewModel.CategoriaQuantidadeItem
                {
                    Categoria = g.Key,
                    Quantidade = g.Count()
                })
                .OrderByDescending(x => x.Quantidade)
                .ToList();

            var dictTags = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var obsLista = beneficiariosDb
                .Where(b => !string.IsNullOrWhiteSpace(b.Observacao))
                .Select(b => b.Observacao!)
                .ToList();

            foreach (var obs in obsLista)
            {
                var partes = obs.Split(new[] { ',', ' ', '-', '/', '|', '\\', '.', '_', ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var parte in partes)
                {
                    if (parte.StartsWith("#") && parte.Length > 1)
                    {
                        var tag = parte.Substring(1);
                        dictTags[tag] = dictTags.GetValueOrDefault(tag, 0) + 1;
                    }
                    else if (parte.Length > 3)
                    {
                        dictTags[parte] = dictTags.GetValueOrDefault(parte, 0) + 1;
                    }
                }
            }

            var necessidadesComunidade = dictTags
                .OrderByDescending(kv => kv.Value)
                .Take(6)
                .Select(kv => new DashboardViewModel.CategoriaQuantidadeItem
                {
                    Categoria = "#" + char.ToUpper(kv.Key[0]) + kv.Key.Substring(1).ToLower(),
                    Quantidade = kv.Value
                })
                .ToList();

            // 5. Evolução Financeira Mensal (Últimos 6 meses + meses com movimentação)
            var mesesReferencia = new SortedSet<(int Ano, int Mes)>();
            for (int i = 5; i >= 0; i--)
            {
                var dt = agora.AddMonths(-i);
                mesesReferencia.Add((dt.Year, dt.Month));
            }
            foreach (var tf in transacoesFin.Where(x => x.DataHora.HasValue))
            {
                mesesReferencia.Add((tf.DataHora!.Value.Year, tf.DataHora!.Value.Month));
            }

            var mesesUltimos = mesesReferencia.TakeLast(8).ToList();
            var mesesLabels = new List<string>();
            var entradasPorMes = new List<decimal>();
            var saidasPorMes = new List<decimal>();
            var doacoesPorMes = new List<decimal>();

            foreach (var (ano, mes) in mesesUltimos)
            {
                var nomeMes = new DateTime(ano, mes, 1).ToString("MMM/yy", culturaPtBr);
                nomeMes = char.ToUpper(nomeMes[0]) + nomeMes.Substring(1).Replace(".", "");
                mesesLabels.Add(nomeMes);

                var doMes = transacoesFin
                    .Where(x => x.DataHora.HasValue && x.DataHora.Value.Year == ano && x.DataHora.Value.Month == mes)
                    .ToList();

                entradasPorMes.Add(doMes.Where(x => x.Razao == "Entrada").Sum(x => x.Valor));
                saidasPorMes.Add(doMes.Where(x => x.Razao == "Saída" || x.Razao == "Saida").Sum(x => x.Valor));
                doacoesPorMes.Add(doMes.Where(x => x.Categoria.Equals("Doação", StringComparison.OrdinalIgnoreCase) || x.Categoria.Equals("Doacao", StringComparison.OrdinalIgnoreCase)).Sum(x => x.Valor));
            }

            // 6. Movimentações por Categoria Financeira
            var categoriasDb = await _context.Categoria.AsNoTracking().ToListAsync();
            var movPorCategoria = categoriasDb
                .Select(cat =>
                {
                    var itensCat = transacoesFin.Where(t => t.Categoria.Equals(cat.Nome, StringComparison.OrdinalIgnoreCase)).ToList();
                    return new DashboardViewModel.CategoriaValorItem
                    {
                        Categoria = cat.Nome,
                        Valor = itensCat.Sum(x => x.Valor),
                        QuantidadeOperacoes = itensCat.Count
                    };
                })
                .OrderByDescending(x => x.Valor)
                .ThenByDescending(x => x.QuantidadeOperacoes)
                .ToList();

            // 7. Próximos Eventos Agendados
            var eventosDb = await _context.Evento
                .AsNoTracking()
                .Include(e => e.ResponsavelNavigation)
                .Include(e => e.VoluntarioAtribuido)
                .OrderByDescending(e => e.data_inicio)
                .ToListAsync();

            var proximosEventos = eventosDb
                .OrderByDescending(e => !e.data_fim.HasValue || e.data_fim.Value >= agora)
                .ThenBy(e => e.data_inicio)
                .Take(6)
                .Select(e =>
            {
                string status = "Agendado";
                if (e.data_fim.HasValue && e.data_fim.Value < agora)
                    status = "Encerrado";
                else if (e.data_inicio.HasValue && e.data_inicio.Value <= agora && (!e.data_fim.HasValue || e.data_fim.Value >= agora))
                    status = "Em Andamento";

                return new DashboardViewModel.EventoProximoItem
                {
                    Id = e.Id,
                    Nome = e.Nome,
                    DataInicio = e.data_inicio,
                    DataFim = e.data_fim,
                    Local = string.IsNullOrWhiteSpace(e.Local) ? "Sede da ONG" : e.Local,
                    Responsavel = e.ResponsavelNavigation?.Nome ?? "Coordenação",
                    TotalVoluntarios = e.VoluntarioAtribuido?.Count ?? 0,
                    Status = status
                };
            }).ToList();

            return new DashboardViewModel
            {
                TotalBeneficiarios = totalBeneficiarios,
                TotalDependentes = totalDependentes,
                TotalVoluntarios = totalVoluntarios,
                EventosAgendados = eventosAgendados,
                TotalEventosRealizados = totalEventosRealizados,
                SaldoAtual = saldoAtual,
                TotalEntradas = totalEntradas,
                TotalSaidas = totalSaidas,
                TotalDoacoes = totalDoacoes,
                DoacoesMesAtual = doacoesMesAtual,
                EntradasMesAtual = entradasMesAtual,
                SaidasMesAtual = saidasMesAtual,
                TotalProdutosCadastrados = totalProdutosCadastrados,
                TotalUnidadesEstoque = totalUnidadesEstoque,
                TotalItensDoadosSaida = totalItensDoadosSaida,
                MesesLabels = mesesLabels,
                EntradasPorMes = entradasPorMes,
                SaidasPorMes = saidasPorMes,
                DoacoesPorMes = doacoesPorMes,
                InventarioPorCategoria = inventarioPorCategoria,
                BeneficiariosPorSituacao = beneficiariosPorSituacao,
                MovimentacoesPorCategoriaFinanceira = movPorCategoria,
                NecessidadesComunidade = necessidadesComunidade,
                UltimasTransacoes = transacoesFin.Take(8).ToList(),
                ProximosEventos = proximosEventos,
                Carteiras = carteiras
            };
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
    }
}