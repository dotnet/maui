using System.Collections.Generic;
using Microsoft.Maui.Controls;

namespace Recipes.Models
{
    public class Item
    {
        public string Id { get; set; }
        public string RecipeName { get; set; }
        public string ImageUrl { get; set; }
        public List<Ingredient> Ingredients { get; set; }
        public string RecipeBody { get; set; }
        public string RecipeUrl { get; set; }
        public float RecipeRating { get; set; }
        public string RecipeReview { get; set; }
    }
}