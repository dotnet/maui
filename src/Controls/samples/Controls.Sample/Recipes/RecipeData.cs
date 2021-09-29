using Newtonsoft.Json;

namespace Recipes
{
    public class RecipeData
    {
        [JsonProperty("q")]
        public string QueryText { get; set; }

        [JsonProperty("from")]
        public int StartIndex { get; set; }

        [JsonProperty("to")]
        public int EndIndex { get; set; }

        [JsonProperty("hits")]
        public Hit[] Hits { get; set; }
    }

    public class Hit
    {
        [JsonProperty("recipe")]
        public Recipe Recipe { get; set; }

        public int Id { get; set; }
    }

    public class Recipe
    {
        [JsonProperty("label")]
        public string RecipeName { get; set; }

        [JsonProperty("ingredientLines")]
        public string[] Ingredients { get; set; }

        [JsonProperty("image")]
        public string ImageUrl { get; set; }

        [JsonProperty("url")]
        public string RecipeUrl { get; set; }
    }
}
