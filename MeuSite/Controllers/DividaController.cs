using Microsoft.AspNetCore.Mvc;
using MeuSite.Data;
using Microsoft.EntityFrameworkCore;
using MeuSite.Models.Entities;

namespace MeuSite.Controllers
{
    public class DividaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DividaController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? controleAnoId)
        {
            var dividas = await _context.Dividas
                .Include(d => d.ControleAno)
                .Include(d => d.Categoria)
                .Include(d => d.Parcelas)
                .Where(d => !controleAnoId.HasValue || d.ControleAnoId == controleAnoId.Value)
                .OrderByDescending(d => d.DataPrimeiroVencimento)
                .ToListAsync();

            ViewBag.ControleAnoId = controleAnoId;
            ViewBag.ControleAnos = await _context.ControleAnos.ToListAsync();
            ViewBag.Categorias = await _context.Categorias.ToListAsync();

            return View(dividas);
        }

        public async Task<IActionResult> Create(int? controleAnoId)
        {
            ViewBag.ControleAnoId = controleAnoId;
            ViewBag.ControleAnos = await _context.ControleAnos.ToListAsync();
            ViewBag.Categorias = await _context.Categorias.ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Divida divida)
        {
            if (ModelState.IsValid)
            {
                _context.Add(divida);
                await _context.SaveChangesAsync();

                // Criar parcelas automaticamente
                await CriarParcelas(divida);

                return RedirectToAction(nameof(Index), new { controleAnoId = divida.ControleAnoId });
            }

            ViewBag.ControleAnos = await _context.ControleAnos.ToListAsync();
            ViewBag.Categorias = await _context.Categorias.ToListAsync();
            return View(divida);
        }

        private async Task CriarParcelas(Divida divida)
        {
            decimal valorParcela = divida.ValorTotal / divida.Parcelas;
            
            for (int i = 1; i <= divida.Parcelas; i++)
            {
                var parcela = new ParcelaDivida
                {
                    DividaId = divida.Id,
                    NumeroParcela = i,
                    Valor = Math.Round(valorParcela, 2),
                    DataVencimento = divida.DataPrimeiroVencimento.AddMonths(i - 1),
                    Paga = false
                };
                
                _context.ParcelaDividas.Add(parcela);
            }
            
            await _context.SaveChangesAsync();
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var divida = await _context.Dividas
                .Include(d => d.Categoria)
                .Include(d => d.Parcelas)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (divida == null)
            {
                return NotFound();
            }

            ViewBag.ControleAnos = await _context.ControleAnos.ToListAsync();
            ViewBag.Categorias = await _context.Categorias.ToListAsync();
            return View(divida);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Divida divida)
        {
            if (id != divida.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(divida);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DividaExists(divida.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { controleAnoId = divida.ControleAnoId });
            }

            ViewBag.ControleAnos = await _context.ControleAnos.ToListAsync();
            ViewBag.Categorias = await _context.Categorias.ToListAsync();
            return View(divida);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var divida = await _context.Dividas
                .Include(d => d.ControleAno)
                .Include(d => d.Categoria)
                .Include(d => d.Parcelas)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (divida == null)
            {
                return NotFound();
            }

            return View(divida);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var divida = await _context.Dividas
                .Include(d => d.Parcelas)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (divida != null)
            {
                // Remover parcelas primeiro
                _context.ParcelaDividas.RemoveRange(divida.Parcelas);
                _context.Dividas.Remove(divida);
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction(nameof(Index), new { controleAnoId = divida?.ControleAnoId });
        }

        public async Task<IActionResult> PagarParcela(int id)
        {
            var parcela = await _context.ParcelaDividas
                .Include(p => p.Divida)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (parcela == null)
            {
                return NotFound();
            }

            parcela.Paga = true;
            parcela.DataPagamento = DateTime.Now;
            
            _context.Update(parcela);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = parcela.DividaId });
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var divida = await _context.Dividas
                .Include(d => d.ControleAno)
                .Include(d => d.Categoria)
                .Include(d => d.Parcelas.OrderBy(p => p.NumeroParcela))
                .FirstOrDefaultAsync(d => d.Id == id);

            if (divida == null)
            {
                return NotFound();
            }

            return View(divida);
        }

        private bool DividaExists(int id)
        {
            return _context.Dividas.Any(e => e.Id == id);
        }
    }
}
