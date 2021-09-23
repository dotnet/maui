using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Recipes
{
    public class RestService
    {
        HttpClient _client;

        public RestService()
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri("https://api.edamam.com/");
        }

        public async Task<RecipeData> GetRecipeDataAsync(string uri)
        {
            RecipeData recipeData = null;

            try
            {
                HttpResponseMessage response = await _client.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    recipeData = JsonConvert.DeserializeObject<RecipeData>(content);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\tERROR {0}", ex.Message);
            }

            return recipeData;
        }
    }
}