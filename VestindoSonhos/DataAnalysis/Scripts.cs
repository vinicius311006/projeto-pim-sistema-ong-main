using System.Diagnostics;

namespace DataAnalysis
{
    public class GraficoService
    {
        public void GerarGraficos()
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = @"\\Scriptdatagraf1.py",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(processInfo))
            {
                process.WaitForExit();

                string erro = process.StandardError.ReadToEnd();

                if (!string.IsNullOrEmpty(erro))
                {
                    throw new Exception(erro);
                }
            }
        }
    }

}