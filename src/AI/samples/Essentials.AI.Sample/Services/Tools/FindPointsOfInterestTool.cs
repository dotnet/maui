using System.ComponentModel;
using Maui.Controls.Sample.Models;

namespace Maui.Controls.Sample.Services.Tools;

public class FindPointsOfInterestTool(Landmark landmark, LandmarkDataService landmarkService)
{
	[DisplayName("findPointsOfInterest")]
	[Description("Finds points of interest for a landmark.")]
	public async Task<string> Call(
		[Description("This is the type of destination to look up for.")]
		PointOfInterestCategory pointOfInterest,
		[Description("The natural language query of what to search for.")]
		string naturalLanguageQuery)
	{
		var searchResults = await landmarkService.SearchPointsOfInterestAsync(pointOfInterest, naturalLanguageQuery);
		if (searchResults.Count == 0)
		{
			return $"I couldn't find any {pointOfInterest} in {landmark.Name}.";
		}

		var topMatches = searchResults.Select(x => $" - {x.Name}: {x.Description}");

		return
			$"""
            Here are the top {pointOfInterest}s in {landmark.Name} matching '{naturalLanguageQuery}':

            {string.Join(Environment.NewLine, topMatches)}
            """;
	}
}
