using MeuSite.Models.Entities;
using MeuSite.Data;
using Microsoft.EntityFrameworkCore;

namespace MeuSite.Services
{
    public class CalculoService
    {
        private readonly ApplicationDbContext _context;

        public CalculoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<decimal> CalcularTotalReceitasAsync(int controleAnoId)
        {
            return await _context.Receitas
                .Where(r => r.ControleAnoId == controleAnoId)
                .SumAsync(r => r.Valor);
        }

        public async Task<decimal> CalcularTotalDespesasAsync(int controleAnoId)
        {
            return await _context.Despesas
                .Where(d => d.ControleAnoId == controleAnoId)
                .SumAsync(d => d.Valor);
        }

        public async Task<decimal> CalcularTotalDividasAbertasAsync(int controleAnoId)
        {
            return await _context.ParcelaDividas
                .Include(p => p.Divida)
                .Where(p => !p.Paga && p.Divida.ControleAnoId == controleAnoId)
                .SumAsync(p => p.Valor);
        }

        public async Task<decimal> CalcularTotalDividasPagasAsync(int controleAnoId)
        {
            return await _context.ParcelaDividas
                .Include(p => p.Divida)
                .Where(p => p.Paga && p.Divida.ControleAnoId == controleAnoId)
                .SumAsync(p => p.Valor);
        }

        public async Task<decimal> CalcularSaldoAsync(int controleAnoId)
        {
            var totalReceitas = await CalcularTotalReceitasAsync(controleAnoId);
            var totalDespesas = await CalcularTotalDespesasAsync(controleAnoId);
            var totalDividasAbertas = await CalcularTotalDividasAbertasAsync(controleAnoId);

            return totalReceitas - totalDespesas - totalDividasAbertas;
        }

        public async Task<Dictionary<string, decimal>> CalcularTotalPorCategoriaAsync(int controleAnoId)
        {
            var resultado = new Dictionary<string, decimal>();

            // Calcular totais de receitas por categoria
            var receitasPorCategoria = await _context.Receitas
                .Include(r => r.Categoria)
                .Where(r => r.ControleAnoId == controleAnoId && r.Categoria != null)
                .GroupBy(r => r.Categoria.Nome)
                .ToDictionaryAsync(g => g.Key, g => g.Sum(r => r.Valor));

            // Calcular totais de despesas por categoria
            var despesasPorCategoria = await _context.Despesas
                .Include(d => d.Categoria)
                .Where(d => d.ControleAnoId == controleAnoId && d.Categoria != null)
                .GroupBy(d => d.Categoria.Nome)
                .ToDictionaryAsync(g => g.Key, g => g.Sum(d => d.Valor));

            // Calcular totais de dívidas por categoria
            var dividasPorCategoria = await _context.ParcelaDividas
                .Include(p => p.Divida)
                .ThenInclude(d => d.Categoria)
                .Where(p => !p.Paga && p.Divida.ControleAnoId == controleAnoId && p.Divida.Categoria != null)
                .GroupBy(p => p.Divida.Categoria.Nome)
                .ToDictionaryAsync(g => g.Key, g => g.Sum(p => p.Valor));

            // Combinar todos os resultados
            foreach (var kvp in receitasPorCategoria)
            {
                resultado[kvp.Key] = resultado.GetValueOrDefault(kvp.Key) + kvp.Value;
            }

            foreach (var kvp in despesasPorCategoria)
            {
                resultado[kvp.Key] = resultado.GetValueOrDefault(kvp.Key) + kvp.Value;
            }

            foreach (var kvp in dividasPorCategoria)
            {
                resultado[kvp.Key] = resultado.GetValueOrDefault(kvp.Key) + kvp.Value;
            }

            return resultado;
        }

        public async Task<Dictionary<string, decimal>> CalcularPercentualPorCategoriaAsync(int controleAnoId)
        {
            var totaisPorCategoria = await CalcularTotalPorCategoriaAsync(controleAnoId);
            var totalReceitas = await CalcularTotalReceitasAsync(controleAnoId);

            if (totalReceitas == 0)
                return new Dictionary<string, decimal>();

            var percentuais = new Dictionary<string, decimal>();
            foreach (var kvp in totaisPorCategoria)
            {
                percentuais[kvp.Key] = (kvp.Value / totalReceitas) * 100;
            }

            return percentuais;
        }

        public async Task<List<(string Categoria, decimal Valor, decimal Percentual)>> ObterResumoCategoriasAsync(int controleAnoId)
        {
            var totaisPorCategoria = await CalcularTotalPorCategoriaAsync(controleAnoId);
            var totalReceitas = await CalcularTotalReceitasAsync(controleAnoId);

            var resultado = totaisPorCategoria
                .Select(kvp => (
                    Categoria: kvp.Key,
                    Valor: kvp.Value,
                    Percentual: totalReceitas > 0 ? (kvp.Value / totalReceitas) * 100 : 0
                ))
                .OrderByDescending(x => x.Valor)
                .ToList();

            return resultado;
        }

        public async Task<ResumoAnual> ObterResumoAnualAsync(int controleAnoId)
        {
            var totalReceitas = await CalcularTotalReceitasAsync(controleAnoId);
            var totalDespesas = await CalcularTotalDespesasAsync(controleAnoId);
            var totalDividasAbertas = await CalcularTotalDividasAbertasAsync(controleAnoId);
            var totalDividasPagas = await CalcularTotalDividasPagasAsync(controleAnoId);
            var saldo = await CalcularSaldoAsync(controleAnoId);

            return new ResumoAnual
            {
                TotalReceitas = totalReceitas,
                TotalDespesas = totalDespesas,
                TotalDividasAbertas = totalDividasAbertas,
                TotalDividasPagas = totalDividasPagas,
                Saldo = saldo,
                ResumoPorCategoria = await ObterResumoCategoriasAsync(controleAnoId)
            };
        }

        public async Task<List<ParcelaVencimento>> ObterParcelasProximasVencimentoAsync(int controleAnoId, int dias = 30)
        {
            var dataLimite = DateTime.Now.AddDays(dias);

            return await _context.ParcelaDividas
                .Include(p => p.Divida)
                .Where(p => !p.Paga 
                           && p.Divida.ControleAnoId == controleAnoId 
                           && p.DataVencimento <= dataLimite
                           && p.DataVencimento >= DateTime.Now)
                .OrderBy(p => p.DataVencimento)
                .Select(p => new ParcelaVencimento
                {
                    Id = p.Id,
                    DescricaoDivida = p.Divida.Descricao,
                    NumeroParcela = p.NumeroParcela,
                    Valor = p.Valor,
                    DataVencimento = p.DataVencimento,
                    DiasAteVencimento = (int)(p.DataVencimento - DateTime.Now).TotalDays
                })
                .ToListAsync();
        }
    }

    public class ResumoAnual
    {
        public decimal TotalReceitas { get; set; }
        public decimal TotalDespesas { get; set; }
        public decimal TotalDividasAbertas { get; set; }
        public decimal TotalDividasPagas { get; set; }
        public decimal Saldo { get; set; }
        public List<(string Categoria, decimal Valor, decimal Percentual)> ResumoPorCategoria { get; set; } = new();
    }

    public class ParcelaVencimento
    {
        public int Id { get; set; }
        public string DescricaoDivida { get; set; } = string.Empty;
        public int NumeroParcela { get; set; }
        public decimal Valor { get; set; }
        public DateTime DataVencimento { get; set; }
        public int DiasAteVencimento { get; set; }
    }
}
