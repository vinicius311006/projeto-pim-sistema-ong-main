using Xunit;
using VestindoSonhos.Helpers;

namespace VestindoSonhos.Tests
{
    public class MoedaHelperTests
    {
        [Theory(DisplayName = "Valida a conversão correta de strings monetárias em decimal")]
        [InlineData("1000", 1000.0)]
        [InlineData("1000.00", 1000.0)]
        [InlineData("1500.00", 1500.0)]
        [InlineData("1500,00", 1500.0)]
        [InlineData("1.500,00", 1500.0)]
        [InlineData("1,500.00", 1500.0)]
        [InlineData("1500.50", 1500.5)]
        [InlineData("1500,50", 1500.5)]
        [InlineData("1.500,50", 1500.5)]
        [InlineData("1,500.50", 1500.5)]
        [InlineData("10.5", 10.5)]
        [InlineData("10,5", 10.5)]
        [InlineData("1.500", 1500.0)]
        [InlineData("10.000", 10000.0)]
        [InlineData("R$ 1.500,00", 1500.0)]
        [InlineData(" R$ 1500.00 ", 1500.0)]
        [InlineData("", 0.0)]
        [InlineData(null, 0.0)]
        public void ConverterValorMonetario_DeveConverterCorretamente(string? input, double esperadoDouble)
        {
            decimal esperado = (decimal)esperadoDouble;
            decimal resultado = MoedaHelper.ConverterValorMonetario(input);
            Assert.Equal(esperado, resultado);
        }
    }
}
