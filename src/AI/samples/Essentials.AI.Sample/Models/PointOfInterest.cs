using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;

namespace Maui.Controls.Sample.Models;

public class PointOfInterest
{
	public string Name { get; set; } = string.Empty;

	public PointOfInterestCategory Category { get; set; } = PointOfInterestCategory.None;

	public string Description { get; set; } = string.Empty;

	/// <summary>
	/// Embedding vectors generated from the name and description for semantic search.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyList<Embedding<float>>? Embeddings { get; set; }
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
