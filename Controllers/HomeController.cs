using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeSite.Data;
using RecipeSite.Models;
using System.Diagnostics;
using System.Threading.Tasks;

namespace RecipeSite.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Ін'єктуємо контекст бази даних у головний контролер
        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Беремо з бази останні 6 рецептів для відображення на вітрині
            var latestRecipes = await _context.Recipes.Take(6).ToListAsync();
            return View(latestRecipes);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}