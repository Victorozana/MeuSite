using Microsoft.AspNetCore.Mvc;
using MeuSite.Data;
using Microsoft.EntityFrameworkCore;
using MeuSite.Models.Entities;

namespace MeuSite.Controllers
{
    public class ControleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ControleController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var controles = await _context.ControleAnos
                .Include(c => c.Categorias)
                .Include(c => c.Receitas)
                .Include(c => c.Despesas)
                .Include(c => c.Dividas)
                .ToListAsync();

            return View(controles);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ControleAno controle)
        {
            if (ModelState.IsValid)
            {
                _context.Add(controle);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(controle);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var controle = await _context.ControleAnos.FindAsync(id);
            if (controle == null)
            {
                return NotFound();
            }
            return View(controle);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ControleAno controle)
        {
            if (id != controle.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(controle);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ControleAnoExists(controle.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(controle);
        }

        private bool ControleAnoExists(int id)
        {
            return _context.ControleAnos.Any(e => e.Id == id);
        }
    }
}
