using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MeuSite.Data;
using Microsoft.EntityFrameworkCore;
using MeuSite.Models.Entities;

namespace MeuSite.Controllers
{
    public class ReceitaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReceitaController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? controleAnoId)
        {
            var receitas = await _context.Receitas
                .Include(r => r.ControleAno)
                .Include(r => r.Categoria)
                .Include(r => r.Subcategoria)
                .Where(r => !controleAnoId.HasValue || r.ControleAnoId == controleAnoId.Value)
                .OrderByDescending(r => r.Data)
                .ToListAsync();

            ViewBag.ControleAnoId = controleAnoId;
            ViewBag.ControleAnos = await _context.ControleAnos.ToListAsync();
            ViewBag.Categorias = await _context.Categorias.ToListAsync();

            return View(receitas);
        }

        public async Task<IActionResult> Create(int? controleAnoId)
        {
            ViewBag.ControleAnoId = controleAnoId;
            ViewBag.ControleAnos = new SelectList(await _context.ControleAnos.ToListAsync(), "Id", "Nome");
            ViewBag.Categorias = new SelectList(await _context.Categorias.ToListAsync(), "Id", "Nome");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Receita receita)
        {
            if (ModelState.IsValid)
            {
                _context.Add(receita);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { controleAnoId = receita.ControleAnoId });
            }

            ViewBag.ControleAnos = new SelectList(await _context.ControleAnos.ToListAsync(), "Id", "Nome");
            ViewBag.Categorias = new SelectList(await _context.Categorias.ToListAsync(), "Id", "Nome");
            return View(receita);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var receita = await _context.Receitas
                .Include(r => r.Categoria)
                .Include(r => r.Subcategoria)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (receita == null)
            {
                return NotFound();
            }

            ViewBag.ControleAnos = new SelectList(await _context.ControleAnos.ToListAsync(), "Id", "Nome");
            ViewBag.Categorias = new SelectList(await _context.Categorias.ToListAsync(), "Id", "Nome");
            return View(receita);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Receita receita)
        {
            if (id != receita.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(receita);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReceitaExists(receita.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { controleAnoId = receita.ControleAnoId });
            }

            ViewBag.ControleAnos = new SelectList(await _context.ControleAnos.ToListAsync(), "Id", "Nome");
            ViewBag.Categorias = new SelectList(await _context.Categorias.ToListAsync(), "Id", "Nome");
            return View(receita);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var receita = await _context.Receitas
                .Include(r => r.ControleAno)
                .Include(r => r.Categoria)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (receita == null)
            {
                return NotFound();
            }

            return View(receita);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var receita = await _context.Receitas.FindAsync(id);
            if (receita != null)
            {
                _context.Receitas.Remove(receita);
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction(nameof(Index), new { controleAnoId = receita?.ControleAnoId });
        }

        private bool ReceitaExists(int id)
        {
            return _context.Receitas.Any(e => e.Id == id);
        }
    }
}
