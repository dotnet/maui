namespace Microsoft.Maui.Essentials.AI.UnitTests;

/// <summary>
/// Integration tests that verify the complete pipeline: 
/// JsonStreamChunker → StreamingJsonDeserializer produces the same result as parsing the final JSON directly.
/// </summary>
public partial class JsonStreamingRoundtripTests
{
	/// <summary>
	/// Test model for simple roundtrip tests.
	/// </summary>
	private record SimpleModel
	{
		public string? Name { get; init; }
		public string? Description { get; init; }
		public int? Age { get; init; }
		public bool? Active { get; init; }
	}

	/// <summary>
	/// Test model matching the itinerary structure in test JSONL files.
	/// Uses nullable properties to handle partial streaming data.
	/// </summary>
	private record ItineraryModel
	{
		public string? Title { get; init; }
		public string? Description { get; init; }
		public string? DestinationName { get; init; }
		public string? Rationale { get; init; }
		public List<DayPlanModel>? Days { get; init; }
	}

	private record DayPlanModel
	{
		public string? Title { get; init; }
		public string? Subtitle { get; init; }
		public string? Destination { get; init; }
		public List<ActivityModel>? Activities { get; init; }
	}

	private record ActivityModel
	{
		public string? Type { get; init; }
		public string? Title { get; init; }
		public string? Description { get; init; }
	}
}
