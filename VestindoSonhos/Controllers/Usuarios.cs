using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Differencing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VestindoSonhos.Models;
using VestindoSonhos.ViewModels;

namespace VestindoSonhos.Controllers
{
    public class Usuarios : Controller
    {
        private readonly VestindosonhosdbContext _context;

        public Usuarios(VestindosonhosdbContext context)
        {
            _context = context;
        }

        // GET: Usuarios
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Index()
        {
            var usuarios = await (from u in _context.Usuario
                            join c in _context.Cargo on u.Cargo equals c.Id
                            orderby u.Id descending
                            select new HomeUsuarioViewModel
                            {
                                Nome = u.Nome,
                                CargoNome = c.Nome,
                                Telefone = u.Telefone,
                                Email = u.Email,
                                Id = u.Id,
                                ExigirTrocaSenha = u.ExigirTrocaSenha
                            }).ToListAsync();

            ViewBag.TotalUsuarios = usuarios.Count;
            ViewBag.TotalAdmins = usuarios.Count(u => u.CargoNome == "Administrador");
            ViewBag.TotalColaboradores = usuarios.Count(u => u.CargoNome == "Colaborador");
            ViewBag.Cargos = await _context.Cargo.OrderBy(c => c.Nome).ToListAsync();

            return View(usuarios);
        }


        // GET: Usuarios/Details/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuario
                .Include(u => u.CargoNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // GET: Usuarios/Criar
        [Authorize(Roles = "Administrador")]
        public IActionResult Criar()
        {
            CarregarDropdowns();
            return View();
        }
        private void CarregarDropdowns()
        {
            ViewData["Cargo"] = new SelectList(_context.Cargo, "Id", "Nome");
        }

        // POST: Usuarios/Criar
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar([Bind("Nome,Cargo,DataNascimento,Telefone,Email,Endereco,Cpf,Senha,ExigirTrocaSenha")] UsuarioViewModel usuario)
        {
            if (!string.IsNullOrEmpty(usuario.Telefone))
            {
                usuario.Telefone = Regex.Replace(usuario.Telefone, @"\D", "");
                if (usuario.Telefone.Length > 11)
                    usuario.Telefone = usuario.Telefone.Substring(0, 11);
            }
            if (!string.IsNullOrEmpty(usuario.Cpf))
            {
                usuario.Cpf = Regex.Replace(usuario.Cpf, @"\D", "");
                if (usuario.Cpf.Length > 11)
                    usuario.Cpf = usuario.Cpf.Substring(0, 11);
            }
            if (!string.IsNullOrEmpty(usuario.Nome) && usuario.Nome.Length > 50)
            {
                usuario.Nome = usuario.Nome.Substring(0, 50);
            }
            if (!string.IsNullOrEmpty(usuario.Email) && usuario.Email.Length > 40)
            {
                usuario.Email = usuario.Email.Substring(0, 40);
            }
            if (!string.IsNullOrEmpty(usuario.Endereco) && usuario.Endereco.Length > 80)
            {
                usuario.Endereco = usuario.Endereco.Substring(0, 80);
            }

            CarregarDropdowns();
            foreach (var erro in ModelState)
            {
                foreach (var subErro in erro.Value.Errors)
                {
                    Console.WriteLine($"{erro.Key}: {subErro.ErrorMessage}");
                }
            }
            if (ModelState.IsValid)
            {
                if(usuario.Senha.Length < 8)
                {
                    ModelState.AddModelError(nameof(usuario.Senha), "Insira uma senha com mais de 8 caracteres.");
                    CarregarDropdowns();
                    return View(usuario);
                }
                var cargoId = usuario.Cargo;
                var cargoEntity = await _context.Cargo.FirstOrDefaultAsync(c => c.Id == cargoId);
                if (cargoEntity == null)
                {
                    ModelState.AddModelError(nameof(usuario.Cargo), "Cargo inválido.");
                    CarregarDropdowns();
                    return View(usuario);
                }
                var passwordHasher = new PasswordHasher<Usuario>();

                var novo_usuario = new Usuario
                {
                    Nome = usuario.Nome,
                    Cargo = cargoEntity.Id,
                    DataNascimento = usuario.DataNascimento,
                    Telefone = usuario.Telefone,
                    Email = usuario.Email,
                    Endereco = usuario.Endereco,
                    Cpf = usuario.Cpf,
                    ExigirTrocaSenha = usuario.ExigirTrocaSenha
                };
                var senhaCriptografada = passwordHasher.HashPassword(novo_usuario, usuario.Senha);
                novo_usuario.Senha = senhaCriptografada;
                try
                {
                    _context.Add(novo_usuario);
                    await _context.SaveChangesAsync();
                    TempData["Sucesso"] = $"Novo usuário '{novo_usuario.Nome}' cadastrado com sucesso!";
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                
                return RedirectToAction(nameof(Index));
            }
            CarregarDropdowns();
            return View(usuario);
        }

        // GET: Usuarios/Edit/5
        [Authorize(Roles = "Administrador")]
        public IActionResult Editar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var usuario = (from u in _context.Usuario
                           join c in _context.Cargo on u.Cargo equals c.Id
                             where u.Id == id
                             select new EditUsuariosViewModel
                             {
                                 Id = u.Id,
                                 Nome = u.Nome,
                                 Cargo = c.Id,
                                 DataNascimento = u.DataNascimento,
                                 Telefone = u.Telefone,
                                 Email = u.Email,
                                 Endereco = u.Endereco,
                                 ExigirTrocaSenha = u.ExigirTrocaSenha
                             }).FirstOrDefault();
            if (usuario == null)
            {
                return NotFound();
            }
            CarregarDropdowns();
            return View(usuario);
        }

        // POST: Usuarios/Edit/5
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, [Bind("Id,Nome,Cargo,DataNascimento,Telefone,Email,Endereco,NovaSenha,ConfirmarNovaSenha,ExigirTrocaSenha")] EditUsuariosViewModel usuario)
        {
            if (id != usuario.Id)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(usuario.Telefone))
            {
                usuario.Telefone = Regex.Replace(usuario.Telefone, @"\D", "");
                if (usuario.Telefone.Length > 11)
                    usuario.Telefone = usuario.Telefone.Substring(0, 11);
            }
            if (!string.IsNullOrEmpty(usuario.Nome) && usuario.Nome.Length > 50)
            {
                usuario.Nome = usuario.Nome.Substring(0, 50);
            }
            if (!string.IsNullOrEmpty(usuario.Email) && usuario.Email.Length > 40)
            {
                usuario.Email = usuario.Email.Substring(0, 40);
            }
            if (!string.IsNullOrEmpty(usuario.Endereco) && usuario.Endereco.Length > 80)
            {
                usuario.Endereco = usuario.Endereco.Substring(0, 80);
            }

            if (!string.IsNullOrWhiteSpace(usuario.NovaSenha))
            {
                if (usuario.NovaSenha.Length < 6)
                {
                    ModelState.AddModelError(nameof(usuario.NovaSenha), "A nova senha deve ter no mínimo 6 caracteres.");
                }
                if (usuario.NovaSenha != usuario.ConfirmarNovaSenha)
                {
                    ModelState.AddModelError(nameof(usuario.ConfirmarNovaSenha), "As senhas não coincidem.");
                }
            }

            if (ModelState.IsValid)
            {
                var usuario_bd = await _context.Usuario.FirstOrDefaultAsync(u => u.Id == id);
                if (usuario_bd == null)
                {
                    return NotFound();
                }

                var cargoEntity = await _context.Cargo.FirstOrDefaultAsync(c => c.Id == usuario.Cargo);
                if (cargoEntity == null)
                {
                    ModelState.AddModelError(nameof(usuario.Cargo), "Cargo inválido.");
                    CarregarDropdowns();
                    return View(usuario);
                }

                usuario_bd.Nome = usuario.Nome;
                usuario_bd.Cargo = usuario.Cargo;
                usuario_bd.DataNascimento = usuario.DataNascimento;
                usuario_bd.Telefone = usuario.Telefone;
                usuario_bd.Email = usuario.Email;
                usuario_bd.Endereco = usuario.Endereco;
                usuario_bd.ExigirTrocaSenha = usuario.ExigirTrocaSenha;

                if (!string.IsNullOrWhiteSpace(usuario.NovaSenha))
                {
                    var passwordHasher = new PasswordHasher<Usuario>();
                    usuario_bd.Senha = passwordHasher.HashPassword(usuario_bd, usuario.NovaSenha);
                    TempData["Sucesso"] = $"Usuário '{usuario_bd.Nome}' atualizado com nova senha definida!";
                }
                else
                {
                    TempData["Sucesso"] = $"Usuário '{usuario_bd.Nome}' atualizado com sucesso!";
                }

                _context.Update(usuario_bd);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            CarregarDropdowns();
            return View(usuario);
        }

        // GET: Usuarios/RedefinirSenha/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> RedefinirSenha(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuario
                .Include(u => u.CargoNavigation)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
            {
                return NotFound();
            }

            var model = new RedefinirSenhaUsuarioViewModel
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                CargoNome = usuario.CargoNavigation?.Nome ?? "Usuário",
                ExigirTrocaSenha = true
            };

            return View(model);
        }

        // POST: Usuarios/RedefinirSenha
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RedefinirSenha(RedefinirSenhaUsuarioViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var usuario = await _context.Usuario.FindAsync(model.Id);
            if (usuario == null)
            {
                return NotFound();
            }

            var passwordHasher = new PasswordHasher<Usuario>();
            usuario.Senha = passwordHasher.HashPassword(usuario, model.NovaSenha);
            usuario.ExigirTrocaSenha = model.ExigirTrocaSenha;
            _context.Update(usuario);
            await _context.SaveChangesAsync();

            if (model.ExigirTrocaSenha)
            {
                TempData["Sucesso"] = $"Senha do usuário '{usuario.Nome}' redefinida para '{model.NovaSenha}' (o usuário precisará cadastrar uma nova senha no próximo login)!";
            }
            else
            {
                TempData["Sucesso"] = $"Senha do usuário '{usuario.Nome}' redefinida com sucesso para: {model.NovaSenha}";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Usuarios/Delete/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Apagar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuario
                .Include(u => u.CargoNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // POST: Usuarios/Delete/5
        [Authorize(Roles = "Administrador")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuario = await _context.Usuario.FindAsync(id);
            if (usuario != null)
            {
                _context.Usuario.Remove(usuario);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuario.Any(e => e.Id == id);
        }
    }
}
