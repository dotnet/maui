using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;

namespace Maui.Controls.Sample.Models;

public class PointOfInterest
{
	public string Name { get; set; } = string.Empty;

	public PointOfInterestCategory Category { get; set; } = PointOfInterestCategory.None;

	public string Description { get; set; } = string.Empty;

	[JsonIgnore]
	public Embedding<float>? Embedding { get; set; }
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
