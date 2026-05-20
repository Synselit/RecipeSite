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

        // Реєстрація таблиць у базі даних
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<RecipeIngredient> RecipeIngredients { get; set; }

        // Налаштування зв'язків за допомогою Fluent API
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Створюємо композитний первинний ключ (RecipeId + IngredientId)
            modelBuilder.Entity<RecipeIngredient>()
                .HasKey(ri => new { ri.RecipeId, ri.IngredientId });

            // 2. Налаштовуємо зв'язок "Один рецепт -> Багато інгредієнтів у проміжній таблиці"
            modelBuilder.Entity<RecipeIngredient>()
                .HasOne(ri => ri.Recipe)
                .WithMany(r => r.RecipeIngredients)
                .HasForeignKey(ri => ri.RecipeId);

            // 3. Налаштовуємо зв'язок "Один інгредієнт -> Багато рецептів у проміжній таблиці"
            modelBuilder.Entity<RecipeIngredient>()
                .HasOne(ri => ri.Ingredient)
                .WithMany(i => i.RecipeIngredients)
                .HasForeignKey(ri => ri.IngredientId);

                // Вказуємо точність для кількості інгредієнтів (до 2 знаків після коми)
modelBuilder.Entity<RecipeIngredient>()
    .Property(ri => ri.Amount)
    .HasColumnType("decimal(18, 2)");
        }
    }
}