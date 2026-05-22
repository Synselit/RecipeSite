using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeSite.Data;
using RecipeSite.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;


namespace RecipeSite.Controllers
{
    public class RecipesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment; // Додали змінну для роботи з папками

        // Оновили конструктор
        public RecipesController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
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
        public async Task<IActionResult> Create([Bind("Title,Description,CookingTime")] Recipe recipe, IFormFile? uploadFile)
        {
            if (ModelState.IsValid)
            {
                // Якщо користувач завантажив файл
                if (uploadFile != null)
                {
                    // Шлях до публічної папки wwwroot
                    string wwwRootPath = _hostEnvironment.WebRootPath;
                    
                    // Генеруємо унікальне ім'я файлу, щоб картинки з однаковими назвами не перезаписували одна одну
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(uploadFile.FileName);
                    
                    // Створюємо папку images, якщо її ще немає
                    string imageFolder = Path.Combine(wwwRootPath, "images");
                    Directory.CreateDirectory(imageFolder);

                    // Повний шлях, куди фізично збережеться файл
                    string filePath = Path.Combine(imageFolder, fileName);

                    // Копіюємо файл у папку
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await uploadFile.CopyToAsync(fileStream);
                    }

                    // Записуємо відносний шлях у нашу базу даних
                    recipe.ImagePath = "/images/" + fileName;
                }

                _context.Add(recipe);
                await _context.SaveChangesAsync();
                
                return RedirectToAction(nameof(Index)); 
            }
            return View(recipe);
        }
        // GET: Відкриває сторінку з детальною інформацією про один рецепт
        // GET: Відкриває сторінку з детальною інформацією про один рецепт
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            // Шукаємо рецепт і ОДРАЗУ підтягуємо всі його інгредієнти
            var recipe = await _context.Recipes
                .Include(r => r.RecipeIngredients)
                .ThenInclude(ri => ri.Ingredient)
                .FirstOrDefaultAsync(m => m.RecipeId == id);
                
            if (recipe == null) return NotFound();

            // Передаємо список УСІХ інгредієнтів бази у випадаючий список (Select List)
            ViewBag.AllIngredients = new SelectList(_context.Ingredients, "IngredientId", "Name");

            return View(recipe);
        }
        // GET: Відкриває сторінку редагування
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe == null) return NotFound();

            return View(recipe);
        }

        // POST: Зберігає оновлені дані в базу
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RecipeId,Title,Description,CookingTime")] Recipe recipe)
        {
            if (id != recipe.RecipeId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(recipe);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Recipes.Any(e => e.RecipeId == recipe.RecipeId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index)); // Повертаємось до списку
            }
            return View(recipe);
        }
        // GET: Відкриває сторінку з питанням "Ви впевнені?"
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var recipe = await _context.Recipes
                .FirstOrDefaultAsync(m => m.RecipeId == id);
                
            if (recipe == null) return NotFound();

            return View(recipe);
        }

        // POST: Фізично видаляє запис із бази даних
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe != null)
            {
                _context.Recipes.Remove(recipe);
                await _context.SaveChangesAsync(); // Зберігаємо зміни в базі
            }
            
            return RedirectToAction(nameof(Index)); // Повертаємось до списку
        }
       
        // POST: Додає інгредієнт до конкретного рецепта
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddIngredient(int recipeId, int ingredientId, decimal amount, string unit) // <-- Додали string unit
        {
            var exists = await _context.RecipeIngredients
                .AnyAsync(ri => ri.RecipeId == recipeId && ri.IngredientId == ingredientId);

            if (!exists)
            {
                var recipeIngredient = new RecipeIngredient
                {
                    RecipeId = recipeId,
                    IngredientId = ingredientId,
                    Amount = amount,
                    Unit = unit // <-- Передаємо одиниці виміру в базу!
                };

                _context.RecipeIngredients.Add(recipeIngredient);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id = recipeId });
        }
        
        // POST: Видаляє інгредієнт зі складу рецепта
        [HttpPost]
        public async Task<IActionResult> RemoveIngredient(int recipeId, int ingredientId)
        {
            // Шукаємо цей зв'язок у базі
            var recipeIngredient = await _context.RecipeIngredients
                .FirstOrDefaultAsync(ri => ri.RecipeId == recipeId && ri.IngredientId == ingredientId);

            if (recipeIngredient != null)
            {
                _context.RecipeIngredients.Remove(recipeIngredient);
                await _context.SaveChangesAsync(); // Видаляємо назавжди
            }

            // Повертаємося на сторінку деталей
            return RedirectToAction(nameof(Details), new { id = recipeId });
        }
    }
}