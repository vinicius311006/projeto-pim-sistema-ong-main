using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using VestindoSonhos.Models;
using VestindoSonhos.ViewModels;

namespace VestindoSonhos.Controllers
{
    public class Home : Controller
    {
        private readonly VestindosonhosdbContext _context;

        public Home(VestindosonhosdbContext context)
        {
            _context = context;


        }
        private void GerarGraficos()
        {
            var pythonPath = @".\DataAnalysis\.venv\Scripts\python.exe";
            if (!System.IO.File.Exists(pythonPath)) return;

            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = pythonPath,
                    Arguments = @".\DataAnalysis\Scriptdatagraf1.py",
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
                Console.WriteLine($"Erro ao gerar gráficos Home: {ex.Message}");
            }
        }
        public async Task<IActionResult> Index()
        {
            //GerarGraficos();

            var necessidades_banco = await (
                    (from b in _context.Beneficiario
                     where b.Observacao != "" && b.Observacao != null
                     select b.Observacao
                        ).ToListAsync());
            necessidades_banco.AddRange(["#Comida", "#Roupas Frio", "#Comida", "#Calças", "#Calças", "#Meias", "#Calças", "Melancia", "Melancia"]);
            List<string> lista_necessidades = [];
            var dict_necessidades = new Dictionary<string, int>();
            var dict_ordenado = new Dictionary<string, int>();
            foreach(var item in necessidades_banco)
            {
                var necessidades_separadas = item.Split([',', ' ', '-', '/', '|', '\\', '.', '_']);
                foreach(var necessidade in necessidades_separadas)
                {
                    if (necessidade[0] == '#')
                    {
                        var necessidade_sem_identificador = necessidade[1..].ToLower();
                        if (!dict_necessidades.ContainsKey(necessidade_sem_identificador))
                        {
                            dict_necessidades[necessidade_sem_identificador] = 1;
                        }
                        else
                        {
                            dict_necessidades[necessidade_sem_identificador] += 1;
                        }
                        dict_ordenado = dict_necessidades
                        .OrderByDescending(n => n.Value)
                        .Take(8)
                        .ToDictionary(x => x.Key, x => x.Value);
                    }                    
                }
            };

            var contexto = new HomeViewModel
            {
                projetosAtivos = await _context.Evento.Where(e => e.data_fim >= DateTime.Now).CountAsync(),

                voluntariosCadastrados = await _context.Voluntario.CountAsync(),

                eventosPendentesMesVigente = await _context.Evento
                    .Where(e => e.data_inicio.Value.Month == DateTime.Now.Month)
                    .Where(e => e.data_inicio.Value.Year == DateTime.Now.Year)
                    .Where(e => e.data_inicio >= DateTime.Now)
                    .CountAsync(),

                ultimoEvento = await _context.Evento.Where(e => e.data_fim < DateTime.Now).OrderByDescending(e => e.data_fim).FirstOrDefaultAsync(),

                proximosEventos = await _context.Evento.Where(e => e.data_fim >= DateTime.Now).OrderBy(e => e.data_inicio).Take(3).ToListAsync(),

                necessidades = dict_ordenado

            };
            return View(contexto);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}