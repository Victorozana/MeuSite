using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MeuSite.Data;
using Microsoft.EntityFrameworkCore;
using MeuSite.Models.Entities;

namespace MeuSite.Controllers
{
    public class CategoriaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriaController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? controleAnoId)
        {
            var categorias = await _context.Categorias
                .Include(c => c.ControleAno)
                .Include(c => c.Subcategorias)
                .Where(c => !controleAnoId.HasValue || c.ControleAnoId == controleAnoId.Value)
                .OrderBy(c => c.Nome)
                .ToListAsync();

            ViewBag.ControleAnoId = controleAnoId;
            ViewBag.ControleAnos = await _context.ControleAnos.ToListAsync();

            return View(categorias);
        }

        public async Task<IActionResult> Create(int? controleAnoId)
        {
            ViewBag.ControleAnoId = controleAnoId;
            ViewBag.ControleAnos = new SelectList(await _context.ControleAnos.ToListAsync(), "Id", "Nome", controleAnoId);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Categoria categoria)
        {
            ModelState.Remove(nameof(Categoria.ControleAno));
            ModelState.Remove(nameof(Categoria.Subcategorias));

            if (ModelState.IsValid)
            {
                _context.Add(categoria);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { controleAnoId = categoria.ControleAnoId });
            }

            ModelState.AddModelError(string.Empty, "Não foi possível salvar a categoria. Verifique os campos e tente novamente.");
            ViewBag.ControleAnoId = categoria.ControleAnoId;
            ViewBag.ControleAnos = new SelectList(await _context.ControleAnos.ToListAsync(), "Id", "Nome", categoria.ControleAnoId);
            return View(categoria);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoria = await _context.Categorias
                .Include(c => c.ControleAno)
                .Include(c => c.Subcategorias)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (categoria == null)
            {
                return NotFound();
            }

            ViewBag.ControleAnos = new SelectList(await _context.ControleAnos.ToListAsync(), "Id", "Nome");
            return View(categoria);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Categoria categoria)
        {
            if (id != categoria.Id)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(Categoria.ControleAno));
            ModelState.Remove(nameof(Categoria.Subcategorias));

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(categoria);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoriaExists(categoria.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { controleAnoId = categoria.ControleAnoId });
            }

            ViewBag.ControleAnos = new SelectList(await _context.ControleAnos.ToListAsync(), "Id", "Nome");
            return View(categoria);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoria = await _context.Categorias
                .Include(c => c.ControleAno)
                .Include(c => c.Subcategorias)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (categoria == null)
            {
                return NotFound();
            }

            return View(categoria);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria != null)
            {
                _context.Categorias.Remove(categoria);
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction(nameof(Index), new { controleAnoId = categoria?.ControleAnoId });
        }

        // Subcategorias
        public async Task<IActionResult> CreateSubcategoria(int categoriaId)
        {
            var categoria = await _context.Categorias.FindAsync(categoriaId);
            if (categoria == null)
            {
                return NotFound();
            }

            ViewBag.Categoria = categoria;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSubcategoria(Subcategoria subcategoria, int categoriaId)
        {
            subcategoria.CategoriaId = categoriaId;

            if (ModelState.IsValid)
            {
                _context.Add(subcategoria);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Edit), new { id = categoriaId });
            }

            var categoria = await _context.Categorias.FindAsync(categoriaId);
            ViewBag.Categoria = categoria;
            return View(subcategoria);
        }

        public async Task<IActionResult> DeleteSubcategoria(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subcategoria = await _context.Subcategorias
                .Include(s => s.Categoria)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (subcategoria == null)
            {
                return NotFound();
            }

            return View(subcategoria);
        }

        [HttpPost, ActionName("DeleteSubcategoria")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSubcategoriaConfirmed(int id)
        {
            var subcategoria = await _context.Subcategorias.FindAsync(id);
            if (subcategoria != null)
            {
                _context.Subcategorias.Remove(subcategoria);
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction(nameof(Edit), new { id = subcategoria?.CategoriaId });
        }

        private bool CategoriaExists(int id)
        {
            return _context.Categorias.Any(e => e.Id == id);
        }
    }
}
