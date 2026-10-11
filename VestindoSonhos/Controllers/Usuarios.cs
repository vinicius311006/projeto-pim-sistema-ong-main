using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Differencing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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
                                Id = u.Id
                            }).ToListAsync();

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
        public async Task<IActionResult> Criar([Bind("Nome,Cargo,DataNascimento,Telefone,Email,Endereco,Cpf,Senha")] UsuarioViewModel usuario)
        {
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
                };
                var senhaCriptografada = passwordHasher.HashPassword(novo_usuario, usuario.Senha);
                novo_usuario.Senha = senhaCriptografada;
                try
                {
                    _context.Add(novo_usuario);
                    await _context.SaveChangesAsync();
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
                                 Endereco = u.Endereco
                             }).FirstOrDefault();
            if (usuario == null)
            {
                return NotFound();
            }
            CarregarDropdowns();
            return View(usuario);
        }

        // POST: Usuarios/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, [Bind("Id,Nome,Cargo,DataNascimento,Telefone,Email,Endereco")] EditUsuariosViewModel usuario)
        {
            Console.WriteLine(usuario.Id);
            if (id != usuario.Id)
            {
                return NotFound();
            }

            foreach (var erro in ModelState)
            {
                foreach (var subErro in erro.Value.Errors)
                {
                    Console.WriteLine($"{erro.Key}: {subErro.ErrorMessage}");
                }
            }
            if (ModelState.IsValid)
                {
                    var usuario_bd = await _context.Usuario.FirstOrDefaultAsync(u => u.Id == id);
                    var cargoId = usuario.Cargo;
                    var cargoEntity = await _context.Cargo.FirstOrDefaultAsync(c => c.Id == cargoId);
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
                    _context.Update(usuario_bd);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                CarregarDropdowns();
                return View(usuario);
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
