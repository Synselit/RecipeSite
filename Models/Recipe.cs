using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RecipeSite.Models
{
    public class Recipe
    {
        [Key]
        public int RecipeId { get; set; }

        [Required(ErrorMessage = "Введіть назву рецепта")]
        [StringLength(200)]
        public string Title { get; set; }

        [Required(ErrorMessage = "Опис не може бути порожнім")]
        public string Description { get; set; }

        [Display(Name = "Час приготування (хв)")]
        public int CookingTime { get; set; }

        // Зв'язок Many-to-Many через проміжну таблицю
        public List<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
     
   
         public string? ImagePath { get; set; }

         // Зв'язок з категорією
        public int? CategoryId { get; set; }
        public virtual Category? Category { get; set; }
    
    // ...
    }
}