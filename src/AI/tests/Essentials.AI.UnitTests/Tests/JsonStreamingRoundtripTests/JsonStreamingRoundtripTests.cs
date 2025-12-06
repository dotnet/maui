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
}
