using MeuSite.Models.Entities;
using MeuSite.Data;
using Microsoft.EntityFrameworkCore;

namespace MeuSite.Services
{
    public class RelatorioService
    {
        private readonly ApplicationDbContext _context;
        private readonly CalculoService _calculoService;

        public RelatorioService(ApplicationDbContext context, CalculoService calculoService)
        {
            _context = context;
            _calculoService = calculoService;
        }

        public async Task<RelatorioMensal> GerarRelatorioMensalAsync(int controleAnoId, int ano, int mes)
        {
            var dataInicio = new DateTime(ano, mes, 1);
            var dataFim = dataInicio.AddMonths(1).AddDays(-1);

            var receitas = await _context.Receitas
                .Include(r => r.Categoria)
                .Where(r => r.ControleAnoId == controleAnoId && r.Data >= dataInicio && r.Data <= dataFim)
                .ToListAsync();

            var despesas = await _context.Despesas
                .Include(d => d.Categoria)
                .Where(d => d.ControleAnoId == controleAnoId && d.Data >= dataInicio && d.Data <= dataFim)
                .ToListAsync();

            var parcelasDividas = await _context.ParcelaDividas
                .Include(p => p.Divida)
                .ThenInclude(d => d.Categoria)
                .Where(p => !p.Paga && p.Divida.ControleAnoId == controleAnoId && p.DataVencimento >= dataInicio && p.DataVencimento <= dataFim)
                .ToListAsync();

            return new RelatorioMensal
            {
                Ano = ano,
                Mes = mes,
                Periodo = $"{dataInicio:dd/MM/yyyy} a {dataFim:dd/MM/yyyy}",
                TotalReceitas = receitas.Sum(r => r.Valor),
                TotalDespesas = despesas.Sum(d => d.Valor),
                TotalDividas = parcelasDividas.Sum(p => p.Valor),
                Saldo = receitas.Sum(r => r.Valor) - despesas.Sum(d => d.Valor) - parcelasDividas.Sum(p => p.Valor),
                ReceitasPorCategoria = receitas.GroupBy(r => r.Categoria?.Nome ?? "Sem Categoria")
                    .ToDictionary(g => g.Key, g => g.Sum(r => r.Valor)),
                DespesasPorCategoria = despesas.GroupBy(d => d.Categoria?.Nome ?? "Sem Categoria")
                    .ToDictionary(g => g.Key, g => g.Sum(d => d.Valor)),
                DividasPorCategoria = parcelasDividas.GroupBy(p => p.Divida.Categoria?.Nome ?? "Sem Categoria")
                    .ToDictionary(g => g.Key, g => g.Sum(p => p.Valor))
            };
        }

        public async Task<List<RelatorioMensal>> GerarRelatorioAnualCompletoAsync(int controleAnoId)
        {
            var controle = await _context.ControleAnos.FindAsync(controleAnoId);
            if (controle == null)
                return new List<RelatorioMensal>();

            var relatorios = new List<RelatorioMensal>();
            
            for (int mes = 1; mes <= 12; mes++)
            {
                var relatorio = await GerarRelatorioMensalAsync(controleAnoId, controle.Ano, mes);
                if (relatorio.TotalReceitas > 0 || relatorio.TotalDespesas > 0 || relatorio.TotalDividas > 0)
                {
                    relatorios.Add(relatorio);
                }
            }

            return relatorios;
        }

        public async Task<RelatorioComparativo> GerarRelatorioComparativoAsync(int controleAnoId1, int controleAnoId2)
        {
            var resumo1 = await _calculoService.ObterResumoAnualAsync(controleAnoId1);
            var resumo2 = await _calculoService.ObterResumoAnualAsync(controleAnoId2);

            var controle1 = await _context.ControleAnos.FindAsync(controleAnoId1);
            var controle2 = await _context.ControleAnos.FindAsync(controleAnoId2);

            return new RelatorioComparativo
            {
                Ano1 = controle1?.Ano ?? 0,
                Ano2 = controle2?.Ano ?? 0,
                ResumoAno1 = resumo1,
                ResumoAno2 = resumo2,
                VariacaoReceitas = CalcularVariacaoPercentual(resumo1.TotalReceitas, resumo2.TotalReceitas),
                VariacaoDespesas = CalcularVariacaoPercentual(resumo1.TotalDespesas, resumo2.TotalDespesas),
                VariacaoSaldo = CalcularVariacaoPercentual(resumo1.Saldo, resumo2.Saldo)
            };
        }

        public async Task<RelatorioEvolucaoMensal> GerarRelatorioEvolucaoMensalAsync(int controleAnoId)
        {
            var controle = await _context.ControleAnos.FindAsync(controleAnoId);
            if (controle == null)
                return new RelatorioEvolucaoMensal();

            var evolucao = new RelatorioEvolucaoMensal
            {
                Ano = controle.Ano,
                Meses = new List<EvolucaoMes>()
            };

            for (int mes = 1; mes <= 12; mes++)
            {
                var relatorio = await GerarRelatorioMensalAsync(controleAnoId, controle.Ano, mes);
                
                evolucao.Meses.Add(new EvolucaoMes
                {
                    Mes = mes,
                    NomeMes = new DateTime(controle.Ano, mes, 1).ToString("MMMM"),
                    Receitas = relatorio.TotalReceitas,
                    Despesas = relatorio.TotalDespesas,
                    Dividas = relatorio.TotalDividas,
                    Saldo = relatorio.Saldo
                });
            }

            return evolucao;
        }

        public async Task<List<CategoriaTop>> ObterTopCategoriasAsync(int controleAnoId, int limite = 10)
        {
            var resumoCategorias = await _calculoService.ObterResumoCategoriasAsync(controleAnoId);
            
            return resumoCategorias
                .Take(limite)
                .Select(x => new CategoriaTop
                {
                    Nome = x.Categoria,
                    Valor = x.Valor,
                    Percentual = x.Percentual
                })
                .ToList();
        }

        public async Task<RelatorioFluxoCaixa> GerarRelatorioFluxoCaixaAsync(int controleAnoId)
        {
            var controle = await _context.ControleAnos.FindAsync(controleAnoId);
            if (controle == null)
                return new RelatorioFluxoCaixa();

            var fluxoCaixa = new RelatorioFluxoCaixa
            {
                Ano = controle.Ano,
                FluxoMensal = new List<FluxoMensal>()
            };

            for (int mes = 1; mes <= 12; mes++)
            {
                var dataInicio = new DateTime(controle.Ano, mes, 1);
                var dataFim = dataInicio.AddMonths(1).AddDays(-1);

                var receitas = await _context.Receitas
                    .Where(r => r.ControleAnoId == controleAnoId && r.Data >= dataInicio && r.Data <= dataFim)
                    .SumAsync(r => r.Valor);

                var despesas = await _context.Despesas
                    .Where(d => d.ControleAnoId == controleAnoId && d.Data >= dataInicio && d.Data <= dataFim)
                    .SumAsync(d => d.Valor);

                var dividasPagas = await _context.ParcelaDividas
                    .Include(p => p.Divida)
                    .Where(p => p.Paga && p.Divida.ControleAnoId == controleAnoId && p.DataPagamento.HasValue && p.DataPagamento.Value >= dataInicio && p.DataPagamento.Value <= dataFim)
                    .SumAsync(p => p.Valor);

                fluxoCaixa.FluxoMensal.Add(new FluxoMensal
                {
                    Mes = mes,
                    NomeMes = new DateTime(controle.Ano, mes, 1).ToString("MMMM"),
                    Entradas = receitas,
                    Saidas = despesas + dividasPagas,
                    Saldo = receitas - (despesas + dividasPagas)
                });
            }

            return fluxoCaixa;
        }

        private decimal CalcularVariacaoPercentual(decimal valorAnterior, decimal valorAtual)
        {
            if (valorAnterior == 0)
                return valorAtual > 0 ? 100 : 0;

            return ((valorAtual - valorAnterior) / valorAnterior) * 100;
        }
    }

    public class RelatorioMensal
    {
        public int Ano { get; set; }
        public int Mes { get; set; }
        public string Periodo { get; set; } = string.Empty;
        public decimal TotalReceitas { get; set; }
        public decimal TotalDespesas { get; set; }
        public decimal TotalDividas { get; set; }
        public decimal Saldo { get; set; }
        public Dictionary<string, decimal> ReceitasPorCategoria { get; set; } = new();
        public Dictionary<string, decimal> DespesasPorCategoria { get; set; } = new();
        public Dictionary<string, decimal> DividasPorCategoria { get; set; } = new();
    }

    public class RelatorioComparativo
    {
        public int Ano1 { get; set; }
        public int Ano2 { get; set; }
        public ResumoAnual ResumoAno1 { get; set; } = new();
        public ResumoAnual ResumoAno2 { get; set; } = new();
        public decimal VariacaoReceitas { get; set; }
        public decimal VariacaoDespesas { get; set; }
        public decimal VariacaoSaldo { get; set; }
    }

    public class RelatorioEvolucaoMensal
    {
        public int Ano { get; set; }
        public List<EvolucaoMes> Meses { get; set; } = new();
    }

    public class EvolucaoMes
    {
        public int Mes { get; set; }
        public string NomeMes { get; set; } = string.Empty;
        public decimal Receitas { get; set; }
        public decimal Despesas { get; set; }
        public decimal Dividas { get; set; }
        public decimal Saldo { get; set; }
    }

    public class CategoriaTop
    {
        public string Nome { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public decimal Percentual { get; set; }
    }

    public class RelatorioFluxoCaixa
    {
        public int Ano { get; set; }
        public List<FluxoMensal> FluxoMensal { get; set; } = new();
    }

    public class FluxoMensal
    {
        public int Mes { get; set; }
        public string NomeMes { get; set; } = string.Empty;
        public decimal Entradas { get; set; }
        public decimal Saidas { get; set; }
        public decimal Saldo { get; set; }
    }
}
