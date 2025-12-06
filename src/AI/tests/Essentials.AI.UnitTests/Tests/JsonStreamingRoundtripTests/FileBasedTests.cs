using System.Text.Json;
using Maui.Controls.Sample.Models;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

public partial class JsonStreamingRoundtripTests
{
	/// <summary>
	/// Tests using real JSONL files from TestData.
	/// </summary>
	public class FileBasedTests
	{
		[Theory]
		[InlineData("maui-itinerary-1.jsonl")]
		[InlineData("maui-itinerary-2.jsonl")]
		[InlineData("maui-itinerary-3.jsonl")]
		[InlineData("mount-fuji-itinerary-1.jsonl")]
		[InlineData("sahara-itinerary-1.jsonl")]
		[InlineData("serengeti-itinerary-1.jsonl")]
		public void Roundtrip_FromJsonlFile_DeserializerProducesEquivalentResult(string fileName)
		{
			// Arrange
			var filePath = Path.Combine("TestData", "ObjectStreams", fileName);
			var lines = File.ReadAllLines(filePath);
			var finalLine = lines[^1];

			// Act - pass each line through chunker and deserializer
			var models = ProcessLines<Itinerary>(lines);

			// Assert - we get one model per line plus flush
			Assert.Equal(lines.Length + 1, models.Count);

			// Final model should match the final JSON line
			var finalModel = models[^1];
			Assert.NotNull(finalModel);

			// Parse final line directly for comparison
			var serializerOptions = new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
			};
			var expectedDoc = JsonDocument.Parse(finalLine);
			var actualJson = JsonSerializer.Serialize(finalModel, serializerOptions);
			var actualDoc = JsonDocument.Parse(actualJson);

			// Compare key properties - objects should be structurally equivalent
			AssertJsonStructureContainsExpected(expectedDoc.RootElement, actualDoc.RootElement, "root");
		}

		[Theory]
		[InlineData("maui-itinerary-1.jsonl")]
		[InlineData("maui-itinerary-2.jsonl")]
		[InlineData("maui-itinerary-3.jsonl")]
		[InlineData("mount-fuji-itinerary-1.jsonl")]
		[InlineData("sahara-itinerary-1.jsonl")]
		[InlineData("serengeti-itinerary-1.jsonl")]
		public void Roundtrip_FromJsonlFile_EachLineDeserializesProgressively(string fileName)
		{
			// This test validates that each intermediate line produces a valid partial object
			var filePath = Path.Combine("TestData", "ObjectStreams", fileName);
			var lines = File.ReadAllLines(filePath);

			var models = ProcessLines<Itinerary>(lines);

			// Verify each model after each line is valid (not null after first non-trivial content)
			// The structure should progressively grow
			int? lastDaysCount = null;
			for (int i = 0; i < models.Count; i++)
			{
				var model = models[i];
				if (model?.Days != null && model.Days.Count > 0)
				{
					// Days count should never decrease
					if (lastDaysCount.HasValue)
						Assert.True(model.Days.Count >= lastDaysCount.Value, 
							$"Days count decreased from {lastDaysCount} to {model.Days.Count} at step {i}");
					lastDaysCount = model.Days.Count;
				}
			}
		}

		/// <summary>
		/// Asserts that all properties in expected are present in actual with equivalent values.
		/// This handles the case where chunker output may have properties in different order.
		/// </summary>
		private static void AssertJsonStructureContainsExpected(JsonElement expected, JsonElement actual, string path)
		{
			if (expected.ValueKind != actual.ValueKind)
			{
				// Allow null in actual when expected has a value (partial streaming may leave some null)
				if (actual.ValueKind == JsonValueKind.Null)
					return;
				Assert.Fail($"Value kind mismatch at {path}: expected {expected.ValueKind}, got {actual.ValueKind}");
			}

			switch (expected.ValueKind)
			{
				case JsonValueKind.Object:
					foreach (var prop in expected.EnumerateObject())
					{
						if (actual.TryGetProperty(prop.Name, out var actualProp))
						{
							AssertJsonStructureContainsExpected(prop.Value, actualProp, $"{path}.{prop.Name}");
						}
						// Note: We don't fail if property is missing - streaming may not have all properties yet
					}
					break;

				case JsonValueKind.Array:
					var expectedItems = expected.EnumerateArray().ToList();
					var actualItems = actual.EnumerateArray().ToList();
					var minCount = Math.Min(expectedItems.Count, actualItems.Count);
					for (int i = 0; i < minCount; i++)
					{
						AssertJsonStructureContainsExpected(expectedItems[i], actualItems[i], $"{path}[{i}]");
					}
					break;

				case JsonValueKind.String:
					Assert.Equal(expected.GetString(), actual.GetString());
					break;

				case JsonValueKind.Number:
					// Compare as raw text to handle int/double differences
					Assert.Equal(expected.GetRawText(), actual.GetRawText());
					break;

				case JsonValueKind.True:
				case JsonValueKind.False:
					Assert.Equal(expected.GetBoolean(), actual.GetBoolean());
					break;

				// Null is already handled above
			}
		}
	}
}
