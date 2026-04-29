using System.Text.Json.Serialization;

namespace Maui.Controls.Sample.Models;

public class PointOfInterest
{
	public string Name { get; set; } = string.Empty;

	public PointOfInterestCategory Category { get; set; } = PointOfInterestCategory.None;

	public string Description { get; set; } = string.Empty;
}

public enum PointOfInterestCategory
{
	None,
	Cafe,
	Campground,
	Hotel,
	Marina,
	Museum,
	NationalMonument,
	Restaurant,
}
