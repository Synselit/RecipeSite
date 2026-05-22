using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeSite.Data;
using RecipeSite.Models;

namespace RecipeSite.Controllers
{
    public class RecipesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public RecipesController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // GET: Усі рецепти
        public async Task<IActionResult> Index()
        {
            return View(await _context.Recipes.ToListAsync());
        }

        // GET: Деталі рецепта
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var recipe = await _context.Recipes
                .Include(r => r.RecipeIngredients)
                .ThenInclude(ri => ri.Ingredient)
                .FirstOrDefaultAsync(m => m.RecipeId == id);

            if (recipe == null) return NotFound();

            return View(recipe);
        }

        // GET: Відкриває сторінку створення
        public IActionResult Create()
        {
            return View();
        }

        // POST: Зберігає новий рецепт
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RecipeId,Title,Description,CookingTime")] Recipe recipe, IFormFile? uploadFile, List<string> ingredientNames, List<decimal> amounts, List<string> units)
        {
            if (ModelState.IsValid)
            {
                if (uploadFile != null)
                {
                    string wwwRootPath = _hostEnvironment.WebRootPath;
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(uploadFile.FileName);
                    string imageFolder = Path.Combine(wwwRootPath, "images");
                    Directory.CreateDirectory(imageFolder);
                    string filePath = Path.Combine(imageFolder, fileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await uploadFile.CopyToAsync(fileStream);
                    }
                    recipe.ImagePath = "/images/" + fileName;
                }

                _context.Add(recipe);
                await _context.SaveChangesAsync();

                // Обробка текстових назв інгредієнтів
                if (ingredientNames != null)
                {
                    for (int i = 0; i < ingredientNames.Count; i++)
                    {
                        var name = ingredientNames[i]?.Trim();
                        if (string.IsNullOrEmpty(name)) continue;

                        // Шукаємо інгредієнт у базі. Якщо немає — створюємо.
                        var ingredient = await _context.Ingredients.FirstOrDefaultAsync(ing => ing.Name == name);
                        if (ingredient == null)
                        {
                            ingredient = new Ingredient { Name = name };
                            _context.Ingredients.Add(ingredient);
                            await _context.SaveChangesAsync(); 
                        }

                        _context.RecipeIngredients.Add(new RecipeIngredient
                        {
                            RecipeId = recipe.RecipeId,
                            IngredientId = ingredient.IngredientId,
                            Amount = amounts[i],
                            Unit = units[i]
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            return View(recipe);
        }

        // GET: Відкриває сторінку редагування
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var recipe = await _context.Recipes
                .Include(r => r.RecipeIngredients)
                .ThenInclude(ri => ri.Ingredient)
                .FirstOrDefaultAsync(m => m.RecipeId == id);

            if (recipe == null) return NotFound();

            return View(recipe);
        }

        // POST: Зберігає відредагований рецепт
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RecipeId,Title,Description,CookingTime,ImagePath")] Recipe recipe, IFormFile? uploadFile, List<string> ingredientNames, List<decimal> amounts, List<string> units)
        {
            if (id != recipe.RecipeId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (uploadFile != null)
                    {
                        string wwwRootPath = _hostEnvironment.WebRootPath;
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(uploadFile.FileName);
                        string imageFolder = Path.Combine(wwwRootPath, "images");
                        Directory.CreateDirectory(imageFolder);
                        string filePath = Path.Combine(imageFolder, fileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await uploadFile.CopyToAsync(fileStream);
                        }
                        recipe.ImagePath = "/images/" + fileName;
                    }

                    _context.Update(recipe);

                    // Видаляємо старі зв'язки
                    var oldIngredients = _context.RecipeIngredients.Where(ri => ri.RecipeId == id);
                    _context.RecipeIngredients.RemoveRange(oldIngredients);

                    // Додаємо нові зі списку
                    if (ingredientNames != null)
                    {
                        for (int i = 0; i < ingredientNames.Count; i++)
                        {
                            var name = ingredientNames[i]?.Trim();
                            if (string.IsNullOrEmpty(name)) continue;

                            var ingredient = await _context.Ingredients.FirstOrDefaultAsync(ing => ing.Name == name);
                            if (ingredient == null)
                            {
                                ingredient = new Ingredient { Name = name };
                                _context.Ingredients.Add(ingredient);
                                await _context.SaveChangesAsync();
                            }

                            _context.RecipeIngredients.Add(new RecipeIngredient
                            {
                                RecipeId = recipe.RecipeId,
                                IngredientId = ingredient.IngredientId,
                                Amount = amounts[i],
                                Unit = units[i]
                            });
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Recipes.Any(e => e.RecipeId == recipe.RecipeId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(recipe);
        }

        // GET: Видалення
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var recipe = await _context.Recipes
                .FirstOrDefaultAsync(m => m.RecipeId == id);
            if (recipe == null) return NotFound();

            return View(recipe);
        }

        // POST: Підтвердження видалення
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe != null)
            {
                _context.Recipes.Remove(recipe);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}