using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeSite.Data;
using RecipeSite.Models;
using System.Threading.Tasks;


namespace RecipeSite.Controllers
{
    public class RecipesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RecipesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var recipes = await _context.Recipes.ToListAsync();
            return View(recipes);
        }
        // GET: Відкриває порожню сторінку з формою
        public IActionResult Create()
        {
            return View();
        }

        // POST: Зберігає дані з форми в базу даних
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,CookingTime")] Recipe recipe)
        {
            if (ModelState.IsValid)
            {
                _context.Add(recipe);
                await _context.SaveChangesAsync();
                
                // Після збереження повертаємось на список рецептів
                return RedirectToAction(nameof(Index)); 
            }
            return View(recipe);
        }
    }
}