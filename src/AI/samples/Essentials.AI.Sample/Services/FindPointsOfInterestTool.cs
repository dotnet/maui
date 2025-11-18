using System.ComponentModel;
using Maui.Controls.Sample.Models;

namespace Maui.Controls.Sample.Services;

public class FindPointsOfInterestTool(Landmark landmark, Dictionary<string, int> toolLookups)
{
    public enum Category
    {
        campground,
        hotel,
        cafe,
        museum,
        marina,
        restaurant,
        nationalMonument
    }

    public static string[] Categories =>
	[
		"campground",
        "hotel",
        "cafe",
        "museum",
        "marina",
        "restaurant",
        "nationalMonument"
    ];

    public static string[] GetSuggestions(Category category)
    {
        return category switch
        {
            Category.restaurant => ["Restaurant 1", "Restaurant 2", "Restaurant 3"],
            Category.campground => ["Campground 1", "Campground 2", "Campground 3"],
            Category.hotel => ["Hotel 1", "Hotel 2", "Hotel 3"],
            Category.cafe => ["Cafe 1", "Cafe 2", "Cafe 3"],
            Category.museum => ["Museum 1", "Museum 2", "Museum 3"],
            Category.marina => ["Marina 1", "Marina 2", "Marina 3"],
            Category.nationalMonument => ["The National Rock 1", "The National Rock 2", "The National Rock 3"],
            _ => Array.Empty<string>()
        };
    }

    [DisplayName("findPointsOfInterest")]
    [Description("Finds points of interest for a landmark.")]
    public string Call(string categoryName, string naturalLanguageQuery)
    {
        toolLookups[categoryName] = toolLookups.GetValueOrDefault(categoryName, 0) + 1;

        if (!Enum.TryParse<Category>(categoryName, true, out var category))
        {
            return $"Invalid category: {categoryName}";
        }

        var suggestions = GetSuggestions(category);
        return $"There are these {categoryName} in {landmark.Name}: {string.Join(", ", suggestions)}";
    }
}
