using System.Text.Json;
using System.Text.Json.Serialization;
using Maui.Controls.Sample.Services;
using Maui.Controls.Sample.Models;
using Microsoft.Maui.Essentials.AI.UnitTests.TestHelpers;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

public partial class StreamingJsonDeserializerTests
{
	public class DiagnosticOutputTests
	{
		private readonly ITestOutputHelper _output;

		public DiagnosticOutputTests(ITestOutputHelper output)
		{
			_output = output;
		}

		[Fact]
		public void ShowJsonAndModelProgressionForMauiItinerary()
		{
			var deserializer = new StreamingJsonDeserializer<Itinerary>();
			var chunks = DataStreamLoader.LoadStream("maui-itinerary-1.txt");
			
			_output.WriteLine($"Total chunks to process: {chunks.Length}");
			_output.WriteLine($"{'=',-120}");
			_output.WriteLine("");
			
			var jsonOptions = new JsonSerializerOptions
			{
				WriteIndented = true,
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
				Converters = { new JsonStringEnumConverter() }
			};

			Itinerary? previousModel = null;
			int changeCount = 0;
			
			for (int i = 0; i < chunks.Length; i++)
			{
				var chunk = chunks[i];
				var currentModel = deserializer.ProcessChunk(chunk);
				
				var isSameInstance = ReferenceEquals(previousModel, currentModel);
				var changeIndicator = isSameInstance ? "SAME" : "NEW ";
				
				if (!isSameInstance)
				{
					changeCount++;
				}

				_output.WriteLine($"[Chunk {i + 1:D3}] {changeIndicator} | Chunk: '{chunk}'");
				_output.WriteLine($"Buffer: {deserializer.PartialJson}");
				
				if (currentModel != null)
				{
					try
					{
						var serialized = JsonSerializer.Serialize(currentModel, jsonOptions);
						_output.WriteLine($"Serialized Model:");
						_output.WriteLine(serialized);
					}
					catch (Exception ex)
					{
						_output.WriteLine($"Failed to serialize: {ex.Message}");
					}
				}
				else
				{
					_output.WriteLine($"Model: NULL");
				}
				
				_output.WriteLine("");
				_output.WriteLine($"{'-',-120}");
				_output.WriteLine("");
				
				previousModel = currentModel;
			}
			
			_output.WriteLine($"{'=',-120}");
			_output.WriteLine($"Final Statistics:");
			_output.WriteLine($"  Total chunks: {chunks.Length}");
			_output.WriteLine($"  Model changes: {changeCount}");
			_output.WriteLine($"  Update rate: {(double)changeCount / chunks.Length * 100:F2}%");
			
			// Assert the test passes
			Assert.True(changeCount >= 100, $"Expected at least 100 model updates, got {changeCount}");
		}
	}
}
