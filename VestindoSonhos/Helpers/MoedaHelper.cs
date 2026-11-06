using System;
using System.Globalization;
using System.Linq;

namespace VestindoSonhos.Helpers
{
    public static class MoedaHelper
    {
        /// <summary>
        /// Converte strings numéricas/monetárias enviadas pelo usuário em decimal seguro,
        /// tratando adequadamente formatos pt-BR (ex: "1.500,00", "1500,00"),
        /// formatos invariantes/HTML5 (ex: "1500.00", "1000.00", "1500.50"),
        /// inteiros ("1500") e símbolos de moeda ("R$ 1.500,00").
        /// Evita o bug onde "1500.00" vira 150000 em ambientes com cultura Linux/Invariant.
        /// </summary>
        public static decimal ConverterValorMonetario(string? valorString)
        {
            if (string.IsNullOrWhiteSpace(valorString))
                return 0m;

            string limpo = valorString.Trim()
                .Replace("R$", "", StringComparison.OrdinalIgnoreCase)
                .Replace(" ", "");

            // 1. Possui ambos os separadores (. e ,)
            // Ex: "1.500,50" (pt-BR) ou "1,500.50" (en-US)
            if (limpo.Contains('.') && limpo.Contains(','))
            {
                if (limpo.LastIndexOf(',') > limpo.LastIndexOf('.'))
                {
                    // pt-BR: ponto é milhar, vírgula é decimal
                    limpo = limpo.Replace(".", "").Replace(',', '.');
                }
                else
                {
                    // en-US: vírgula é milhar, ponto é decimal
                    limpo = limpo.Replace(",", "");
                }
            }
            // 2. Possui apenas vírgula (ex: "1500,50", "1500,00", "1,000,000")
            else if (limpo.Contains(','))
            {
                int totalVirgulas = limpo.Count(c => c == ',');
                if (totalVirgulas > 1)
                {
                    // Múltiplas vírgulas -> milhar en-US
                    limpo = limpo.Replace(",", "");
                }
                else
                {
                    // Vírgula única -> separador decimal pt-BR
                    limpo = limpo.Replace(',', '.');
                }
            }
            // 3. Possui apenas ponto (ex: "1000.00", "1500.00", "1500.50", "1.500", "1.000.000")
            else if (limpo.Contains('.'))
            {
                int totalPontos = limpo.Count(c => c == '.');
                if (totalPontos > 1)
                {
                    // Múltiplos pontos -> milhar pt-BR (ex: 1.000.000)
                    limpo = limpo.Replace(".", "");
                }
                else
                {
                    // Ponto único:
                    // Se possui exatamente 3 dígitos após o ponto (ex: "1.500" ou "10.000"),
                    // no contexto brasileiro isso é separador de milhar.
                    // Se possui 1 ou 2 dígitos (ex: "1000.00", "1500.00", "1500.50", "10.5"),
                    // é claramente separador decimal (centavos).
                    var partes = limpo.Split('.');
                    if (partes.Length == 2 && partes[1].Length == 3 && partes[0].Length >= 1)
                    {
                        limpo = limpo.Replace(".", "");
                    }
                    else
                    {
                        // Mantém o ponto como separador decimal para o InvariantCulture
                    }
                }
            }

            if (decimal.TryParse(limpo, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal resultado))
            {
                return resultado;
            }

            return 0m;
        }
    }
}
