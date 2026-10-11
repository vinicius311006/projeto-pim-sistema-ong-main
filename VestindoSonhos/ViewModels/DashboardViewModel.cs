using System;
using System.Collections.Generic;
using VestindoSonhos.Models;

namespace VestindoSonhos.ViewModels
{
    public partial class DashboardViewModel
    {

        //Movimentações financeiras: Tipo, data e o valor;

        //Doações Financeiras: data e valor;

        //Doações de Roupas: Quantidade e data;

        //Eventos / campanhas:
        //Total de voluntários cadastrados no sistemas e quantidade de voluntários em cada evento ativo.

        public class EstatisticasFinanceiras
        {
            public DateTime? Data { get; set; }

            public decimal Valor { get; set; }

            public string Tipo { get; set; }
            
            public string Razao { get; set; }
        }

        public class EstatisticasFinanceirasDoacao
        {
            public DateTime? Data { get; set; }

            public decimal Valor { get; set; }
        }

        public class EstatisticasDoacoesSaida
        {
            public int Quantidade { get; set; }

            public DateTime? Data { get; set; }
        }

        public class EstatisticasVoluntariosAtribuidos
        {
            public string nomeEvento { get; set; }

            public int voluntariosAtribuidos { get; set; }
        }

        public int TotalVoluntarios { get; set; }
        public int EventosAgendados { get; set; }

        public List<EstatisticasVoluntariosAtribuidos> estatisticasVoluntariosAtribuidos { get; set; }
        public List<EstatisticasFinanceiras> estatisticasFinanceiras { get; set; } = new();
        public List<EstatisticasFinanceirasDoacao> estatisticasFinanceirasDoacao { get; set; } = new();
        public List<EstatisticasDoacoesSaida> estatisticasRoupas { get; set; } = new();

    }
}
