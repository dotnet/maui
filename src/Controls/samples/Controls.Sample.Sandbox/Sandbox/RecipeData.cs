using System.Text.Json.Serialization;

namespace Recipes
{
    public class RecipeData
    {
        [JsonPropertyName("q")]
        public string QueryText { get; set; }

        [JsonPropertyName("from")]
        public int StartIndex { get; set; }

        [JsonPropertyName("to")]
        public int EndIndex { get; set; }

        [JsonPropertyName("hits")]
        public Hit[] Hits { get; set; }
    }

    public class Hit
    {
        [JsonPropertyName("recipe")]
        public Recipe Recipe { get; set; }

        public int Id { get; set; }
    }

    public class Recipe
    {
        [JsonPropertyName("label")]
        public string RecipeName { get; set; }

        [JsonPropertyName("ingredientLines")]
        public string[] Ingredients { get; set; }

        [JsonPropertyName("image")]
        public string ImageUrl { get; set; }

        [JsonPropertyName("url")]
        public string RecipeUrl { get; set; }
    }
}
