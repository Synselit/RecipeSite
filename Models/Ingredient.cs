using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RecipeSite.Models
{
    public class Ingredient
    {
        [Key]
        public int IngredientId { get; set; }

        [Required(ErrorMessage = "Назва інгредієнта обов'язкова")]
        [StringLength(100)]
        public string Name { get; set; } // "Яйця", "Картопля" тощо.

        // Зв'язок Many-to-Many через проміжну таблицю
        public List<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
    }
}