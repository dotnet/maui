using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;

namespace Maui.Controls.Sample.Models;

public record Landmark
{
	public int Id { get; init; }

	public required string Name { get; init; }

	public required string Continent { get; init; }

	public required string Description { get; init; }

	public required string ShortDescription { get; init; }

	public double Latitude { get; init; }

	public double Longitude { get; init; }

	public double Span { get; init; }

	public string? PlaceId { get; init; }

	[JsonIgnore]
	public string BackgroundImageName => $"{Id}";

	[JsonIgnore]
	public string ThumbnailImageName => $"{Id}_thumb";

	[JsonIgnore]
	public Location Location => new(Latitude, Longitude);

	/// <summary>
	/// Embedding vector generated from Name and ShortDescription for RAG search.
	/// </summary>
	[JsonIgnore]
	public Embedding<float>? Embedding { get; set; }
}
