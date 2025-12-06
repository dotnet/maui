using Maui.Controls.Sample.Services;

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
	/// Processes a sequence of JSON lines through chunker and deserializer, returning models after each line.
	/// </summary>
	/// <typeparam name="T">The model type to deserialize to.</typeparam>
	/// <param name="lines">JSON lines representing progressive object construction.</param>
	/// <returns>List of models after each line plus after flush.</returns>
	private static List<T?> ProcessLines<T>(string[] lines) where T : class
	{
		var chunker = new JsonStreamChunker();
		var deserializer = new StreamingJsonDeserializer<T>();
		var models = new List<T?>();

		foreach (var line in lines)
		{
			var chunk = chunker.Process(line);
			models.Add(deserializer.ProcessChunk(chunk));
		}
		models.Add(deserializer.ProcessChunk(chunker.Flush()));

		return models;
	}

	/// <summary>
	/// Processes lines and returns both chunks and models for detailed assertions.
	/// </summary>
	private static (List<string> Chunks, List<T?> Models) ProcessLinesWithChunks<T>(string[] lines) where T : class
	{
		var chunker = new JsonStreamChunker();
		var deserializer = new StreamingJsonDeserializer<T>();
		var chunks = new List<string>();
		var models = new List<T?>();

		foreach (var line in lines)
		{
			var chunk = chunker.Process(line);
			chunks.Add(chunk);
			models.Add(deserializer.ProcessChunk(chunk));
		}
		var flushChunk = chunker.Flush();
		chunks.Add(flushChunk);
		models.Add(deserializer.ProcessChunk(flushChunk));

		return (chunks, models);
	}
}
