using Microsoft.AspNetCore.Mvc;
using MeuSite.Data;
using Microsoft.EntityFrameworkCore;
using MeuSite.Models.Entities;

namespace MeuSite.Controllers
{
    public class DespesaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DespesaController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? controleAnoId)
        {
            var despesas = await _context.Despesas
                .Include(d => d.ControleAno)
                .Include(d => d.Categoria)
                .Include(d => d.Subcategoria)
                .Where(d => !controleAnoId.HasValue || d.ControleAnoId == controleAnoId.Value)
                .OrderByDescending(d => d.Data)
                .ToListAsync();

            ViewBag.ControleAnoId = controleAnoId;
            ViewBag.ControleAnos = await _context.ControleAnos.ToListAsync();
            ViewBag.Categorias = await _context.Categorias.ToListAsync();

            return View(despesas);
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
        public async Task<IActionResult> Create(Despesa despesa)
        {
            if (ModelState.IsValid)
            {
                _context.Add(despesa);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { controleAnoId = despesa.ControleAnoId });
            }

            ViewBag.ControleAnos = await _context.ControleAnos.ToListAsync();
            ViewBag.Categorias = await _context.Categorias.ToListAsync();
            return View(despesa);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var despesa = await _context.Despesas
                .Include(d => d.Categoria)
                .Include(d => d.Subcategoria)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (despesa == null)
            {
                return NotFound();
            }

            ViewBag.ControleAnos = await _context.ControleAnos.ToListAsync();
            ViewBag.Categorias = await _context.Categorias.ToListAsync();
            return View(despesa);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Despesa despesa)
        {
            if (id != despesa.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(despesa);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DespesaExists(despesa.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { controleAnoId = despesa.ControleAnoId });
            }

            ViewBag.ControleAnos = await _context.ControleAnos.ToListAsync();
            ViewBag.Categorias = await _context.Categorias.ToListAsync();
            return View(despesa);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var despesa = await _context.Despesas
                .Include(d => d.ControleAno)
                .Include(d => d.Categoria)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (despesa == null)
            {
                return NotFound();
            }

            return View(despesa);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var despesa = await _context.Despesas.FindAsync(id);
            if (despesa != null)
            {
                _context.Despesas.Remove(despesa);
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction(nameof(Index), new { controleAnoId = despesa?.ControleAnoId });
        }

        private bool DespesaExists(int id)
        {
            return _context.Despesas.Any(e => e.Id == id);
        }
    }
}
