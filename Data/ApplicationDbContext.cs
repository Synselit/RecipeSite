using Microsoft.EntityFrameworkCore;
using RecipeSite.Models;

namespace RecipeSite.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<RecipeIngredient> RecipeIngredients { get; set; }
        public DbSet<Category> Categories { get; set; } // Наша нова таблиця

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ВАЖЛИВО: Повертаємо правило для зв'язку "багато до багатьох"
            modelBuilder.Entity<RecipeIngredient>()
                .HasKey(ri => new { ri.RecipeId, ri.IngredientId });

            // Автоматично додаємо початкові категорії
            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryId = 1, Name = "Перші страви" },
                new Category { CategoryId = 2, Name = "Другі страви" },
                new Category { CategoryId = 3, Name = "Сніданки" },
                new Category { CategoryId = 4, Name = "Десерти" },
                new Category { CategoryId = 5, Name = "Закуски" }
            );
        }
    }
}