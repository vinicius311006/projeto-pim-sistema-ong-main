using System;
using System.IO;
using System.Text.Json;

namespace VestindoSonhos.Helpers
{
    public class SaasConfigModel
    {
        public string NomeOrganizacao { get; set; } = "ONG Vestindo Sonhos";
        public string Cnpj { get; set; } = "12.345.678/0001-90";
        public string EmailInstitucional { get; set; } = "contato@vestindosonhos.org.br";
        public string TelefoneSuporte { get; set; } = "(11) 98765-4321";
        public string EnderecoSede { get; set; } = "Av. Paulista, 1500 - São Paulo/SP";
        public decimal MetaArrecadacaoMensal { get; set; } = 15000.00m;
        public int AlertaEstoqueMinimo { get; set; } = 10;
        public int TempoSessaoMinutos { get; set; } = 30;
        public bool ExigirTrocaSenhaNovosUsuarios { get; set; } = true;
        public bool NotificarNovasDoacoes { get; set; } = true;
        public bool HabilitarApiMobile { get; set; } = true;
    }

    public static class ConfiguracaoSistemaHelper
    {
        private static readonly string SettingsFilePath = Path.Combine(AppContext.BaseDirectory, "saas-settings.json");

        public static SaasConfigModel ObterConfiguracoes()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    var json = File.ReadAllText(SettingsFilePath);
                    var cfg = JsonSerializer.Deserialize<SaasConfigModel>(json);
                    if (cfg != null) return cfg;
                }
            }
            catch { }
            return new SaasConfigModel();
        }

        public static void SalvarConfiguracoes(SaasConfigModel model)
        {
            var json = JsonSerializer.Serialize(model, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsFilePath, json);
        }
    }
}
