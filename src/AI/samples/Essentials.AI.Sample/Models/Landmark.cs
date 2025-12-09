using System.Text.Json.Serialization;

namespace Maui.Controls.Sample.Models;

public record Landmark
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("continent")]
    public required string Continent { get; init; }

    [JsonPropertyName("description")]
    public required string Description { get; init; }

    [JsonPropertyName("shortDescription")]
    public required string ShortDescription { get; init; }

    [JsonPropertyName("latitude")]
    public double Latitude { get; init; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; init; }

    [JsonPropertyName("span")]
    public double Span { get; init; }

    [JsonPropertyName("placeID")]
    public string? PlaceId { get; init; }

    [JsonIgnore]
    public string BackgroundImageName => $"{Id}";

    [JsonIgnore]
    public string ThumbnailImageName => $"{Id}_thumb";

    [JsonIgnore]
    public Location Location => new(Latitude, Longitude);
}
