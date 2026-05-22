using System.Collections.Generic;

namespace RecipeSite.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;

        // Зв'язок: в одній категорії може бути багато рецептів
        public virtual ICollection<Recipe>? Recipes { get; set; }
    }
}