using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.DotNet.Scaffolding.Shared.Project;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using VestindoSonhos.Models;
using VestindoSonhos.ViewModels;

namespace VestindoSonhos.Controllers
{
    public class Eventos : Controller
    {
        private readonly VestindosonhosdbContext _context;

        public Eventos(VestindosonhosdbContext context)
        {
            _context = context;
        }

        // GET: Eventos
        [Authorize(Roles = "Administrador,Colaborador")]
        public async Task<IActionResult> Index()
        {
            var context = new EventosViewModel
            {
                Eventos = await _context.Evento.Include(e => e.ResponsavelNavigation).OrderBy(e => e.data_inicio).Where(e => e.data_fim >= DateTime.Now) .ToListAsync(),
                Voluntarios = await _context.Voluntario.ToListAsync()
            };

            return View(context);
        }

        // GET: Eventos/Details/5
        [Authorize(Roles = "Administrador,Colaborador")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var _evento = await _context.Evento
                .Include(e => e.ResponsavelNavigation)
                .FirstOrDefaultAsync(
                    e => e.Id == id);
            var context = new DetalhesEventoViewModel
            {
                evento = _evento,
                listaVoluntarios = await _context.VoluntarioAtribuido
                .Where(va => va.Evento == id)
                .Select(va => new Voluntario
                {
                    Id = va.VoluntarioNavigation.Id,
                    Nome = va.VoluntarioNavigation.Nome,
                    Telefone = va.VoluntarioNavigation.Telefone
                })
                .ToListAsync()
            };
            ViewData["Responsavel"] = new SelectList(_context.Usuario, "Id", "Nome", _evento.Responsavel);

            if (context == null)
            {
                return NotFound();
            }

            return View(context);
        }


        // GET: Eventos/Criar
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Criar()
        {
            ViewData["Responsavel"] = new SelectList(_context.Usuario.Select(u => new { u.Id, u.Nome }), "Id", "Nome");

            var contexto = new CriacaoEventoViewModel
            {
                listaVoluntarios = await _context.Voluntario
                .Select(v => new CriacaoEventoViewModel.VoluntarioView
                {
                    Id = v.Id,
                    Nome = v.Nome,
                    Telefone = v.Telefone
                }).ToListAsync(),
                evento = new Evento()
            };
            return View(contexto);
        }

        // POST: Eventos/Criar
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar([Bind("evento, idVoluntarios")] CriacaoEventoViewModel model)
        {
            ModelState.Remove("evento.ResponsavelNavigation");
            ModelState.Remove("evento.VoluntarioAtribuido");
            ModelState.Remove("evento.Responsavel");

            if (ModelState.IsValid)
            {
                var idUsuarioClaim = User.FindFirst("UserId")?.Value 
                    ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                int idUsuario = 1;
                if (!string.IsNullOrEmpty(idUsuarioClaim) && int.TryParse(idUsuarioClaim, out int parsedId))
                {
                    idUsuario = parsedId;
                }

                var evento = new Evento
                {
                    Nome = model.evento.Nome,
                    data_inicio = model.evento.data_inicio,
                    data_fim = model.evento.data_fim,
                    Local = model.evento.Local,
                    Observacao = model.evento.Observacao,
                    Responsavel = idUsuario
                };

                _context.Evento.Add(evento);
                await _context.SaveChangesAsync();

                if (model.idVoluntarios != null && model.idVoluntarios.Any())
                {
                    foreach (var idVoluntario in model.idVoluntarios)
                    {
                        _context.VoluntarioAtribuido.Add(new VoluntarioAtribuido
                        {
                            Evento = evento.Id,
                            Voluntario = idVoluntario,
                            DataAdicao = DateTime.Now
                        });
                    }
                    await _context.SaveChangesAsync();
                }
                
                return RedirectToAction(nameof(Index));
            }
            ViewData["Responsavel"] = new SelectList(_context.Usuario.Select(u => new { u.Id, u.Nome }), "Id", "Nome");
            var contexto = new CriacaoEventoViewModel
            {
                evento = model.evento,
                listaVoluntarios = await _context.Voluntario
                    .Select(v => new CriacaoEventoViewModel.VoluntarioView
                    {
                        Id = v.Id,
                        Nome = v.Nome,
                        Telefone = v.Telefone
                    }).ToListAsync()
            };
            return View(contexto);
        }

        // GET: Eventos/Editar/5
        [Authorize(Roles = "Administrador,Colaborador")]
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            ViewData["Responsavel"] = new SelectList(_context.Usuario, "Id", "Nome");
            var contexto = new CriacaoEventoViewModel
            {
                evento = await _context.Evento.FirstOrDefaultAsync(e => e.Id == id),
                listaVoluntarios = await _context.Voluntario
                .Select(v => new CriacaoEventoViewModel.VoluntarioView
                {
                    Id = v.Id,
                    Nome = v.Nome,
                    Telefone = v.Telefone
                }).ToListAsync(),
                idVoluntarios = await _context.VoluntarioAtribuido
                .Where(v => v.Evento == id)
                .Select(v => v.Voluntario)
                .ToListAsync()
            };

            if (contexto == null || contexto.evento == null)
            {
                return NotFound();
            }
            ViewData["Responsavel"] = new SelectList(_context.Usuario.Select(u => new { u.Id, u.Nome }), "Id", "Nome", contexto.evento.Responsavel);
            return View(contexto);
        }

        // POST: Eventos/Editar/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Administrador,Colaborador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, [Bind("evento, idVoluntarios")] CriacaoEventoViewModel model)
        {
            if (id != model.evento.Id)
            {
                Console.WriteLine("{0} {1}" ,id, model.evento.Id);
                return NotFound();
            }
            ModelState.Remove("evento.ResponsavelNavigation");
            ModelState.Remove("evento.VoluntarioAtribuido");

            if (ModelState.IsValid)
            {
                try
                {
                    var evento_atual = await _context.Evento.FirstOrDefaultAsync(e => e.Id == id);
                    if (evento_atual == null)
                    {
                        return NotFound();
                    }

                    evento_atual.Nome = model.evento.Nome;
                    evento_atual.Local = model.evento.Local;
                    evento_atual.data_inicio = model.evento.data_inicio;
                    evento_atual.data_fim = model.evento.data_fim;
                    evento_atual.Observacao = model.evento.Observacao;
                    if (model.evento.Responsavel > 0)
                    {
                        evento_atual.Responsavel = model.evento.Responsavel;
                    }

                    _context.Evento.Update(evento_atual);

                    var voluntarios_atuais = await _context.VoluntarioAtribuido
                        .Where(va => va.Evento == model.evento.Id)
                        .Select(va => va.Voluntario)
                        .ToListAsync();
                    var voluntarios_novos = model.idVoluntarios ?? new List<int>();

                    var listaAdicao = voluntarios_novos.Except(voluntarios_atuais).ToList();
                    var listaRemocao = voluntarios_atuais.Except(voluntarios_novos).ToList();

                    foreach (var id_add in listaAdicao)
                    {
                        _context.VoluntarioAtribuido.Add(new VoluntarioAtribuido
                        {
                            Evento = model.evento.Id,
                            Voluntario = id_add,
                            DataAdicao = DateTime.Now
                        });
                    }

                    if (listaRemocao.Any())
                    {
                        var remover = await _context.VoluntarioAtribuido
                            .Where(va => va.Evento == model.evento.Id && listaRemocao.Contains(va.Voluntario))
                            .ToListAsync();
                        _context.VoluntarioAtribuido.RemoveRange(remover);
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventoExists(model.evento.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["Responsavel"] = new SelectList(_context.Usuario, "Id", "Nome");
            var contexto = new CriacaoEventoViewModel
            {
                evento = new Evento
                {
                    Nome = model.evento.Nome,
                    data_inicio = model.evento.data_inicio,
                    data_fim = model.evento.data_fim,
                    Local = model.evento.Local,
                    Responsavel = model.evento.Responsavel
                },
                listaVoluntarios = await _context.Voluntario
                .Select(v => new CriacaoEventoViewModel.VoluntarioView
                {
                    Id = v.Id,
                    Nome = v.Nome,
                    Telefone = v.Telefone
                }).ToListAsync(),
                idVoluntarios = await _context.VoluntarioAtribuido
                    .Where(v => v.Evento == id)
                    .Select(v => v.Voluntario)
                    .ToListAsync()
                };
            
            return View(model);
        }

        // GET: Eventos/Apagar/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Apagar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var evento = await _context.Evento
                .Include(e => e.ResponsavelNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (evento == null)
            {
                return NotFound();
            }

            return View(evento);
        }

        // POST: Eventos/Apagar/5
        [HttpPost, ActionName("Apagar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var evento = await _context.Evento.FindAsync(id);
            if (evento != null)
            {
                _context.Evento.Remove(evento);
                await _context.VoluntarioAtribuido
                    .Where(va => va.Evento == evento.Id)
                    .ExecuteDeleteAsync();
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EventoExists(int id)
        {
            return _context.Evento.Any(e => e.Id == id);
        }
    }
}
