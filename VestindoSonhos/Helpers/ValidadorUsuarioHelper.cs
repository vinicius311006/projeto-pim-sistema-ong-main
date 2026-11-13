using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace VestindoSonhos.Helpers
{
    public static class ValidadorUsuarioHelper
    {
        /// <summary>
        /// Valida um CPF brasileiro usando o cálculo oficial de módulo 11 dos dois dígitos verificadores.
        /// Rejeita CPFs vazios, com tamanho diferente de 11 dígitos ou sequências repetidas (ex: 11111111111).
        /// </summary>
        public static bool CpfEhValido(string? cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return false;

            var digitos = Regex.Replace(cpf, @"\D", "");
            if (digitos.Length != 11)
                return false;

            // Rejeita CPFs com todos os dígitos iguais (ex: 00000000000, 11111111111, etc.)
            if (digitos.Distinct().Count() == 1)
                return false;

            // Cálculo do 1º dígito verificador
            int soma1 = 0;
            for (int i = 0; i < 9; i++)
            {
                soma1 += (digitos[i] - '0') * (10 - i);
            }
            int resto1 = (soma1 * 10) % 11;
            if (resto1 == 10) resto1 = 0;

            if ((digitos[9] - '0') != resto1)
                return false;

            // Cálculo do 2º dígito verificador
            int soma2 = 0;
            for (int i = 0; i < 10; i++)
            {
                soma2 += (digitos[i] - '0') * (11 - i);
            }
            int resto2 = (soma2 * 10) % 11;
            if (resto2 == 10) resto2 = 0;

            return (digitos[10] - '0') == resto2;
        }

        /// <summary>
        /// Verifica se a data de nascimento informada corresponde a uma pessoa com 18 anos completos ou mais
        /// (e idade máxima plausível de 120 anos).
        /// </summary>
        public static bool EhMaiorDeIdade(DateTime? dataNascimento, DateTime? dataReferencia = null)
        {
            if (!dataNascimento.HasValue)
                return false;

            var hoje = (dataReferencia ?? DateTime.Today).Date;
            var nascimento = dataNascimento.Value.Date;

            if (nascimento > hoje)
                return false;

            int idade = hoje.Year - nascimento.Year;
            if (nascimento > hoje.AddYears(-idade))
            {
                idade--;
            }

            return idade >= 18 && idade <= 120;
        }

        /// <summary>
        /// Verifica se o telefone possui DDD válido (11 a 99) e 10 ou 11 dígitos numéricos (fixo ou celular).
        /// </summary>
        public static bool TelefoneEhValido(string? telefone)
        {
            if (string.IsNullOrWhiteSpace(telefone))
                return false;

            var digitos = Regex.Replace(telefone, @"\D", "");
            if (digitos.Length != 10 && digitos.Length != 11)
                return false;

            // Rejeita sequências repetidas (ex: 11111111111 ou 00000000000)
            if (digitos.Distinct().Count() == 1)
                return false;

            int ddd = int.Parse(digitos.Substring(0, 2));
            return ddd >= 11 && ddd <= 99;
        }
    }
}
