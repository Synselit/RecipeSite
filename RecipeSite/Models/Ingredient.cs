using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RecipeSite.Models;

namespace RecipeSite.Models
{
    public class Ingredient
    {
        [Key]
        public int IngredientId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } // Наприклад: "Яйце", "Молоко", "Борошно"

        // Навігаційна властивість
        public List<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
    }
}