namespace RecipeSite.Models
{
    public class RecipeIngredient
    {
        // Зовнішні ключі
        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; }

        public int IngredientId { get; set; }
        public Ingredient Ingredient { get; set; }

        // Додаткові дані (Payload)
        public decimal Amount { get; set; } // Кількість (наприклад: 500)
        public string Unit { get; set; }    // Одиниця виміру (наприклад: "грам", "шт", "мл")
    }
}