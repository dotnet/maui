using System.ComponentModel;
using Maui.Controls.Sample.Models;

namespace Maui.Controls.Sample.Services.Tools;

public class FindPointsOfInterestTool(Landmark landmark)
{
    public enum Category
    {
        Cafe,
        Campground,
        Hotel,
        Marina,
        Museum,
        NationalMonument,
        Restaurant,
    }

    [DisplayName("findPointsOfInterest")]
    [Description("Finds points of interest for a landmark.")]
    public string Call()
    {
        // var suggestions = GetSuggestions(pointOfInterest);

        // return $"There are these {pointOfInterest} in {landmark.Name}: {string.Join(", ", suggestions)}";
        return "Cats Town " + landmark.Name;
    }

    private static string[] GetSuggestions(Category category) =>
        category switch
        {
            Category.Cafe => ["Cafe 1", "Cafe 2", "Cafe 3"],
            Category.Campground => ["Campground 1", "Campground 2", "Campground 3"],
            Category.Hotel => ["Hotel 1", "Hotel 2", "Hotel 3"],
            Category.Marina => ["Marina 1", "Marina 2", "Marina 3"],
            Category.Museum => ["Museum 1", "Museum 2", "Museum 3"],
            Category.NationalMonument => ["The National Rock 1", "The National Rock 2", "The National Rock 3"],
            Category.Restaurant => ["Restaurant 1", "Restaurant 2", "Restaurant 3"],
            _ => []
        };
}
