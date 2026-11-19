using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace VestindoSonhos.Helpers
{
    /// <summary>
    /// Provedor global de metadados e filtro de validação para garantir que 100% das mensagens
    /// de validação (cliente e servidor) e nomes de campos sejam exibidos em Português (pt-BR).
    /// </summary>
    public class PtBrMetadataProvider : IDisplayMetadataProvider, IValidationMetadataProvider
    {
        private static readonly Dictionary<string, string> NomesAmigaveis = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Nome"] = "Nome",
            ["Telefone"] = "Telefone",
            ["Endereco"] = "Endereço",
            ["Situacao"] = "Situação Social",
            ["Observacao"] = "Observação",
            ["Descricao"] = "Descrição",
            ["Razao"] = "Razão",
            ["Categoria"] = "Categoria",
            ["CategoriaTransacao"] = "Categoria",
            ["Carteira"] = "Carteira",
            ["Valor"] = "Valor",
            ["Hora"] = "Data / Hora",
            ["Cpf"] = "CPF",
            ["Email"] = "E-mail",
            ["Senha"] = "Senha",
            ["SenhaAtual"] = "Senha Atual",
            ["SenhaNova"] = "Nova Senha",
            ["NovaSenha"] = "Nova Senha",
            ["RepeteSenhaNova"] = "Confirmação de Senha",
            ["ConfirmarNovaSenha"] = "Confirmação de Senha",
            ["DataNascimento"] = "Data de Nascimento",
            ["AnoNascimento"] = "Ano de Nascimento",
            ["Idade"] = "Idade",
            ["Sexo"] = "Sexo",
            ["Parentesco"] = "Parentesco",
            ["IdResponsavel"] = "Responsável Familiar",
            ["Cargo"] = "Cargo",
            ["Usuario"] = "Usuário",
            ["Beneficiario"] = "Beneficiário",
            ["Cor"] = "Cor",
            ["Quantidade"] = "Quantidade",
            ["Local"] = "Local",
            ["data_inicio"] = "Data de Início",
            ["data_fim"] = "Data de Término",
            ["DataInicio"] = "Data de Início",
            ["DataFim"] = "Data de Término",
            ["Responsavel"] = "Responsável"
        };

        public static string ObterNomeCampoPtBr(string? nomePropriedade)
        {
            if (string.IsNullOrWhiteSpace(nomePropriedade))
                return "informado";

            var limpo = nomePropriedade.Trim();
            if (limpo.Contains('.'))
                limpo = limpo.Split('.').Last();

            if (NomesAmigaveis.TryGetValue(limpo, out var amigavel))
                return amigavel;

            return limpo;
        }

        public void CreateDisplayMetadata(DisplayMetadataProviderContext context)
        {
            var propName = context.Key.Name;
            if (string.IsNullOrWhiteSpace(propName))
                return;

            var hasDisplayAttr = context.Attributes.OfType<DisplayAttribute>().Any(d => !string.IsNullOrWhiteSpace(d.Name));
            if (!hasDisplayAttr && NomesAmigaveis.TryGetValue(propName, out var nomePtBr))
            {
                context.DisplayMetadata.DisplayName = () => nomePtBr;
            }
        }

        public void CreateValidationMetadata(ValidationMetadataProviderContext context)
        {
            var propInfo = context.Key.PropertyInfo;
            var modelType = context.Key.ModelType;

            // Se for referência não-anulável ou value type não-anulável e ainda não tiver RequiredAttribute,
            // adiciona explicitamente o RequiredAttribute já com mensagem em PT-BR antes do provider padrão em inglês.
            bool jaTemRequired = context.ValidationMetadata.ValidatorMetadata.OfType<RequiredAttribute>().Any();
            if (!jaTemRequired && propInfo != null)
            {
                bool ehValueTypeObrigatorio = modelType.IsValueType && Nullable.GetUnderlyingType(modelType) == null;
                bool ehRefTypeObrigatorio = false;

                if (!modelType.IsValueType && modelType == typeof(string))
                {
                    try
                    {
                        var nullabilityContext = new NullabilityInfoContext();
                        var nullabilityInfo = nullabilityContext.Create(propInfo);
                        ehRefTypeObrigatorio = nullabilityInfo.WriteState == NullabilityState.NotNull;
                    }
                    catch
                    {
                        ehRefTypeObrigatorio = false;
                    }
                }

                if (ehValueTypeObrigatorio || ehRefTypeObrigatorio)
                {
                    context.ValidationMetadata.ValidatorMetadata.Add(new RequiredAttribute
                    {
                        ErrorMessage = "O campo {0} é obrigatório."
                    });
                }
            }

            foreach (var attr in context.ValidationMetadata.ValidatorMetadata.OfType<ValidationAttribute>())
            {
                bool semMensagemCustom = string.IsNullOrWhiteSpace(attr.ErrorMessage)
                    || attr.ErrorMessage.StartsWith("The ", StringComparison.OrdinalIgnoreCase)
                    || attr.ErrorMessage.Contains("field is required", StringComparison.OrdinalIgnoreCase)
                    || attr.ErrorMessage.Contains("is invalid", StringComparison.OrdinalIgnoreCase);

                if (!semMensagemCustom)
                    continue;

                switch (attr)
                {
                    case RequiredAttribute req:
                        req.ErrorMessage = "O campo {0} é obrigatório.";
                        break;
                    case StringLengthAttribute strLen:
                        strLen.ErrorMessage = strLen.MinimumLength > 0
                            ? "O campo {0} deve ter entre {2} e {1} caracteres."
                            : "O campo {0} deve ter no máximo {1} caracteres.";
                        break;
                    case MaxLengthAttribute maxLen:
                        maxLen.ErrorMessage = "O campo {0} deve ter no máximo {1} caracteres.";
                        break;
                    case MinLengthAttribute minLen:
                        minLen.ErrorMessage = "O campo {0} deve ter no mínimo {1} caracteres.";
                        break;
                    case RangeAttribute range:
                        range.ErrorMessage = "O campo {0} deve estar entre {1} e {2}.";
                        break;
                    case EmailAddressAttribute email:
                        email.ErrorMessage = "Informe um endereço de e-mail válido no campo {0}.";
                        break;
                    case PhoneAttribute phone:
                        phone.ErrorMessage = "Informe um telefone válido no campo {0}.";
                        break;
                    case CompareAttribute comp:
                        comp.ErrorMessage = "Os campos {0} e {1} não coincidem.";
                        break;
                    case RegularExpressionAttribute regex:
                        regex.ErrorMessage = "O formato informado no campo {0} é inválido.";
                        break;
                    default:
                        attr.ErrorMessage = "O valor informado para o campo {0} é inválido.";
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Filtro global que intercepta qualquer mensagem residual em inglês no ModelState e traduz para PT-BR.
    /// </summary>
    public class PtBrModelStateFilter : IActionFilter
    {
        private static readonly Regex RegexRequired = new(@"^The\s+(.+?)\s+field\s+is\s+required\.?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex RegexInvalidValue = new(@"^The\s+value\s+'(.*?)'\s+is\s+(?:not\s+valid|invalid)(?:\s+for\s+(.+?))?\.?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex RegexMustBeNumber = new(@"^The\s+field\s+(.+?)\s+must\s+be\s+a\s+number\.?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex RegexMaxLen = new(@"^The\s+field\s+(.+?)\s+must\s+be\s+a\s+string.*?maximum\s+length\s+of\s+'?(\d+)'?\.?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ModelState.IsValid)
                return;

            foreach (var key in context.ModelState.Keys.ToList())
            {
                var entry = context.ModelState[key];
                if (entry == null || entry.Errors.Count == 0)
                    continue;

                var mensagensTraduzidas = new List<string>();
                bool alterou = false;

                foreach (var err in entry.Errors)
                {
                    var msg = err.ErrorMessage ?? string.Empty;
                    var novaMsg = TraduzirMensagemSeIngles(msg, key);
                    if (!string.Equals(msg, novaMsg, StringComparison.Ordinal))
                    {
                        alterou = true;
                    }
                    mensagensTraduzidas.Add(novaMsg);
                }

                if (alterou)
                {
                    entry.Errors.Clear();
                    foreach (var m in mensagensTraduzidas.Distinct())
                    {
                        entry.Errors.Add(m);
                    }
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public static string TraduzirMensagemSeIngles(string mensagem, string? chaveCampo = null)
        {
            if (string.IsNullOrWhiteSpace(mensagem))
                return $"O campo {PtBrMetadataProvider.ObterNomeCampoPtBr(chaveCampo)} é inválido.";

            var mReq = RegexRequired.Match(mensagem);
            if (mReq.Success)
            {
                var campo = PtBrMetadataProvider.ObterNomeCampoPtBr(mReq.Groups[1].Value);
                return $"O campo {campo} é obrigatório.";
            }

            var mInv = RegexInvalidValue.Match(mensagem);
            if (mInv.Success)
            {
                var val = mInv.Groups[1].Value;
                var campoRaw = !string.IsNullOrWhiteSpace(mInv.Groups[2].Value) ? mInv.Groups[2].Value : chaveCampo;
                var campo = PtBrMetadataProvider.ObterNomeCampoPtBr(campoRaw);
                return string.IsNullOrWhiteSpace(val)
                    ? $"O campo {campo} é obrigatório."
                    : $"O valor '{val}' não é válido para o campo {campo}.";
            }

            var mNum = RegexMustBeNumber.Match(mensagem);
            if (mNum.Success)
            {
                var campo = PtBrMetadataProvider.ObterNomeCampoPtBr(mNum.Groups[1].Value);
                return $"O campo {campo} deve ser um número válido.";
            }

            var mLen = RegexMaxLen.Match(mensagem);
            if (mLen.Success)
            {
                var campo = PtBrMetadataProvider.ObterNomeCampoPtBr(mLen.Groups[1].Value);
                var tam = mLen.Groups[2].Value;
                return $"O campo {campo} deve ter no máximo {tam} caracteres.";
            }

            if (mensagem.Equals("A value is required.", StringComparison.OrdinalIgnoreCase) ||
                mensagem.Equals("This field is required.", StringComparison.OrdinalIgnoreCase))
            {
                var campo = PtBrMetadataProvider.ObterNomeCampoPtBr(chaveCampo);
                return $"O campo {campo} é obrigatório.";
            }

            return mensagem;
        }
    }
}
