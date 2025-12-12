using System;
using System.IO;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Maui.Controls.Sample.Models;
using Maui.Controls.Sample.Services;

namespace Microsoft.Maui.Essentials.AI.Benchmarks;

[MemoryDiagnoser]
[GcServer(true)]
[SimpleJob(iterationCount: 20, warmupCount: 5)]
public class StreamingJsonDeserializerBenchmark
{
	private string[]? _chunks;
	private JsonSerializerOptions? _options;

	[GlobalSetup]
	public void Setup()
	{
		// Read the test data stream
		var streamPath = Path.Combine(AppContext.BaseDirectory, "maui-itinerary-1.txt");
		var streamContent = File.ReadAllText(streamPath);
		_chunks = streamContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);

		_options = new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true,
			AllowTrailingCommas = true
		};
	}

	[Benchmark(Description = "StreamingJsonDeserializer (skipDeserialization=true)")]
	public void NewUtf8JsonWriterSkipDeserializationApproach()
	{
		var deserializer = new StreamingJsonDeserializer<TravelItinerary>(_options, skipDeserialization: true);
		
		TravelItinerary? lastModel = null;
		foreach (var chunk in _chunks!)
		{
			var model = deserializer.ProcessChunk(chunk);
			if (model != null)
				lastModel = model;
		}
	}

	[Benchmark(Description = "StreamingJsonDeserializer (skipDeserialization=false)")]
	public void OldStringBuilderSkipDeserializationApproach()
	{
		var deserializer = new StreamingJsonDeserializer<TravelItinerary>(_options, skipDeserialization: false);
		
		TravelItinerary? lastModel = null;
		foreach (var chunk in _chunks!)
		{
			var model = deserializer.ProcessChunk(chunk);
			if (model != null)
				lastModel = model;
		}
	}
}
