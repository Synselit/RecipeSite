namespace RecipeSite.Models
{
    public class RecipeIngredient
    {
        // Зовнішній ключ для Рецепта
        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; }

        // Зовнішній ключ для Інгредієнта
        public int IngredientId { get; set; }
        public Ingredient Ingredient { get; set; }

        // Додаткові поля для рецепта
        public decimal Amount { get; set; } // Кількість (наприклад: 2.5 або 500)
        public string Unit { get; set; }    // Одиниця виміру (наприклад: "шт", "г", "мл")
    }
}