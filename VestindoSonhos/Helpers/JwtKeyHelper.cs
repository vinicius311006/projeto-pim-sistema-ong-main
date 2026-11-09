using System;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;

namespace VestindoSonhos.Helpers
{
    /// <summary>
    /// Provedor centralizado de chave de assinatura JWT.
    /// Evita segredos hardcoded no código-fonte gerando uma chave criptográfica
    /// aleatória de 512 bits (64 bytes) em memória caso nenhuma variável de ambiente esteja definida.
    /// </summary>
    public static class JwtKeyHelper
    {
        private static readonly string _runtimeFallbackKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        public static string ObterChaveJwt(IConfiguration? configuration = null)
        {
            var configKey = configuration?["Jwt:Key"];
            if (!string.IsNullOrWhiteSpace(configKey))
            {
                return configKey;
            }

            var envKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
            if (!string.IsNullOrWhiteSpace(envKey))
            {
                return envKey;
            }

            return _runtimeFallbackKey;
        }
    }
}
