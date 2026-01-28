using Maui.Controls.Sample.Services;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

public partial class JsonMergerTests
{
	/// <summary>
	/// Tests that simulate the real-world translation streaming scenario.
	/// </summary>
	public class TranslationScenario
	{
		[Fact]
		public void MergeOverlay_PartialTranslation_PreservesUntranslatedContent()
		{
			// Original English itinerary
			var baseJson = @"{
				""destination"": ""Maui"",
				""days"": 3,
				""schedule"": [
					{
						""day"": 1,
						""title"": ""Beach Day"",
						""description"": ""Explore the beautiful beaches"",
						""activities"": [
							{""time"": ""9:00 AM"", ""name"": ""Snorkeling"", ""details"": ""Morning snorkeling at the reef""},
							{""time"": ""2:00 PM"", ""name"": ""Sunset Cruise"", ""details"": ""Evening cruise along the coast""}
						]
					},
					{
						""day"": 2,
						""title"": ""Mountain Hike"",
						""description"": ""Hike through the rainforest"",
						""activities"": []
					}
				]
			}";

			// Partial French translation (only first day partially translated)
			var overlayJson = @"{
				""destination"": ""Maui"",
				""schedule"": [
					{
						""title"": ""Journée Plage"",
						""description"": ""Explorez les belles plages"",
						""activities"": [
							{""name"": ""Plongée en apnée"", ""details"": ""Plongée matinale au récif""}
						]
					}
				]
			}";

			var merger = new JsonMerger(baseJson);
			var result = merger.MergeOverlay(overlayJson);
			var parsed = merger.Deserialize<ItineraryModel>(SerializerOptions);

			Assert.NotNull(parsed);
			Assert.Equal("Maui", parsed.Destination);
			Assert.Equal(3, parsed.Days);  // Preserved from base
			Assert.NotNull(parsed.Schedule);
			Assert.Equal(2, parsed.Schedule.Count);  // Both days preserved

			// Day 1 - translated
			var day1 = parsed.Schedule[0];
			Assert.Equal(1, day1.Day);  // Base preserved
			Assert.Equal("Journée Plage", day1.Title);  // Translated
			Assert.Equal("Explorez les belles plages", day1.Description);  // Translated
			Assert.NotNull(day1.Activities);
			Assert.Equal(2, day1.Activities.Count);  // Both activities preserved

			// First activity - partially translated
			Assert.Equal("9:00 AM", day1.Activities[0].Time);  // Base preserved
			Assert.Equal("Plongée en apnée", day1.Activities[0].Name);  // Translated
			Assert.Equal("Plongée matinale au récif", day1.Activities[0].Details);  // Translated

			// Second activity - untranslated (base preserved)
			Assert.Equal("2:00 PM", day1.Activities[1].Time);
			Assert.Equal("Sunset Cruise", day1.Activities[1].Name);
			Assert.Equal("Evening cruise along the coast", day1.Activities[1].Details);

			// Day 2 - completely untranslated
			var day2 = parsed.Schedule[1];
			Assert.Equal(2, day2.Day);
			Assert.Equal("Mountain Hike", day2.Title);  // Base preserved
			Assert.Equal("Hike through the rainforest", day2.Description);  // Base preserved
		}

		[Fact]
		public void MergeOverlay_ProgressiveTranslation_SimulatesStreaming()
		{
			// Original English
			var baseJson = @"{
				""destination"": ""Tokyo"",
				""days"": 2,
				""schedule"": [
					{""day"": 1, ""title"": ""Temple Tour"", ""description"": ""Visit ancient temples""},
					{""day"": 2, ""title"": ""Food Adventure"", ""description"": ""Sample local cuisine""}
				]
			}";

			var merger = new JsonMerger(baseJson);

			// Chunk 1: Just the destination
			var chunk1 = @"{""destination"": ""東京""}";
			merger.MergeOverlay(chunk1);
			var result1 = merger.Deserialize<ItineraryModel>(SerializerOptions);
			Assert.Equal("東京", result1?.Destination);
			Assert.Equal("Temple Tour", result1?.Schedule?[0].Title);  // Still English

			// Chunk 2: First day title translated
			var chunk2 = @"{
				""destination"": ""東京"",
				""schedule"": [
					{""title"": ""寺院ツアー""}
				]
			}";
			merger.MergeOverlay(chunk2);
			var result2 = merger.Deserialize<ItineraryModel>(SerializerOptions);
			Assert.Equal("東京", result2?.Destination);
			Assert.Equal("寺院ツアー", result2?.Schedule?[0].Title);  // Translated
			Assert.Equal("Visit ancient temples", result2?.Schedule?[0].Description);  // Still English
			Assert.Equal("Food Adventure", result2?.Schedule?[1].Title);  // Still English

			// Chunk 3: More translation
			var chunk3 = @"{
				""destination"": ""東京"",
				""schedule"": [
					{""title"": ""寺院ツアー"", ""description"": ""古代の寺院を訪問""},
					{""title"": ""グルメ冒険""}
				]
			}";
			merger.MergeOverlay(chunk3);
			var result3 = merger.Deserialize<ItineraryModel>(SerializerOptions);
			Assert.Equal("古代の寺院を訪問", result3?.Schedule?[0].Description);  // Translated
			Assert.Equal("グルメ冒険", result3?.Schedule?[1].Title);  // Translated
			Assert.Equal("Sample local cuisine", result3?.Schedule?[1].Description);  // Still English
		}

		[Fact]
		public void MergeOverlay_MultipleChunks_AccumulatesCorrectly()
		{
			var baseJson = @"{""name"": ""Hello"", ""greeting"": ""Good morning"", ""farewell"": ""Goodbye""}";
			var merger = new JsonMerger(baseJson);

			// Simulate streaming translation chunks
			var translations = new[]
			{
				@"{""name"": ""Bonjour""}",
				@"{""name"": ""Bonjour"", ""greeting"": ""Bon matin""}",
				@"{""name"": ""Bonjour"", ""greeting"": ""Bon matin"", ""farewell"": ""Au revoir""}"
			};

			foreach (var translation in translations)
			{
				merger.MergeOverlay(translation);
			}

			// Final result should have all translations - verify via deserialization
			var parsed = merger.Deserialize<Dictionary<string, string>>(SerializerOptions);
			Assert.NotNull(parsed);
			Assert.Equal("Bonjour", parsed["name"]);
			Assert.Equal("Bon matin", parsed["greeting"]);
			Assert.Equal("Au revoir", parsed["farewell"]);
		}
	}
}
