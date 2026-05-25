using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RecipeSite.Data;
using RecipeSite.Models;
using Microsoft.AspNetCore.Authorization;

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

        // GET: Усі рецепти з пошуком, холодильником та категоріями
        public async Task<IActionResult> Index(string searchString, string fridgeIngredients, int? categoryId)
        {
            ViewBag.CurrentSearch = searchString;
            ViewBag.FridgeIngredients = fridgeIngredients;
            ViewBag.CurrentCategory = categoryId;

            // Передаємо список усіх категорій для відображення кнопок-вкладок
            ViewBag.Categories = await _context.Categories.ToListAsync();

            var recipesQuery = _context.Recipes
                .Include(r => r.Category)
                .Include(r => r.RecipeIngredients)
                .ThenInclude(ri => ri.Ingredient)
                .AsQueryable();

            // Фільтрація за категорією
            if (categoryId.HasValue)
            {
                recipesQuery = recipesQuery.Where(r => r.CategoryId == categoryId.Value);
            }

            // Звичайний пошук
            if (!string.IsNullOrEmpty(searchString))
            {
                recipesQuery = recipesQuery.Where(r => 
                    r.Title.Contains(searchString) || 
                    r.RecipeIngredients.Any(ri => ri.Ingredient.Name.Contains(searchString)));
            }

            var recipes = await recipesQuery.ToListAsync();

            // Розумний фільтр холодильника
            if (!string.IsNullOrEmpty(fridgeIngredients))
            {
                var userItems = fridgeIngredients
                    .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(i => i.Trim().ToLower())
                    .ToList();

                recipes = recipes
                    .Where(r => r.RecipeIngredients.Any(ri => userItems.Any(ui => ri.Ingredient.Name.ToLower().Contains(ui))))
                    .OrderByDescending(r => r.RecipeIngredients.Count(ri => userItems.Any(ui => ri.Ingredient.Name.ToLower().Contains(ui))))
                    .ToList();
            }

            return View(recipes);
        }

        // GET: Деталі
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var recipe = await _context.Recipes
                .Include(r => r.Category)
                .Include(r => r.RecipeIngredients)
                .ThenInclude(ri => ri.Ingredient)
                .FirstOrDefaultAsync(m => m.RecipeId == id);

            if (recipe == null) return NotFound();

            return View(recipe);
        }


        // GET: Створення
        [Authorize]
        public IActionResult Create()
        {
            // Передаємо список категорій для випадаючого списку (Dropdown)
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name");
            return View();
        }

        // POST: Створення
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("RecipeId,Title,Description,CookingTime,CategoryId")] Recipe recipe, IFormFile? uploadFile, List<string> ingredientNames, List<decimal> amounts, List<string> units)
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
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", recipe.CategoryId);
            return View(recipe);
        }

        // GET: Редагування
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var recipe = await _context.Recipes
                .Include(r => r.RecipeIngredients)
                .ThenInclude(ri => ri.Ingredient)
                .FirstOrDefaultAsync(m => m.RecipeId == id);

            if (recipe == null) return NotFound();

            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", recipe.CategoryId);
            return View(recipe);
        }

        // POST: Редагування
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("RecipeId,Title,Description,CookingTime,ImagePath,CategoryId")] Recipe recipe, IFormFile? uploadFile, List<string> ingredientNames, List<decimal> amounts, List<string> units)
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

                    var oldIngredients = _context.RecipeIngredients.Where(ri => ri.RecipeId == id);
                    _context.RecipeIngredients.RemoveRange(oldIngredients);

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
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", recipe.CategoryId);
            return View(recipe);
        }

        // GET: Видалення
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var recipe = await _context.Recipes.FirstOrDefaultAsync(m => m.RecipeId == id);
            if (recipe == null) return NotFound();

            return View(recipe);
        }

        // POST: Видалення
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
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