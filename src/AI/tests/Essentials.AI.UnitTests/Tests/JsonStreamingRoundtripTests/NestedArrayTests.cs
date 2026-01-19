using Maui.Controls.Sample.Models;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

public partial class JsonStreamingRoundtripTests
{
	/// <summary>
	/// Tests for nested arrays and complex structures.
	/// </summary>
	public class NestedArrayTests
	{
		[Fact]
		public void Roundtrip_NestedArrays_DeserializesCorrectly()
		{
			// Arrange
			var lines = new[]
			{
				"""{"days": []}""",
				"""{"days": [{"activities": []}]}""",
				"""{"days": [{"activities": [{"description": "Hello"}]}]}""",
				"""{"days": [{"activities": [{"description": "Hello World", "title": "Activity 1"}]}]}"""
			};

			// Act
			var models = ProcessLines<Itinerary>(lines);

			// Assert at each step
			Assert.Equal(5, models.Count); // 4 lines + flush

			// After line 1: empty days array
			Assert.NotNull(models[0]);
			Assert.NotNull(models[0]!.Days);
			Assert.Empty(models[0]!.Days!);

			// After line 2: one day with empty activities
			Assert.NotNull(models[1]);
			Assert.Single(models[1]!.Days!);
			Assert.NotNull(models[1]!.Days![0].Activities);
			Assert.Empty(models[1]!.Days![0].Activities!);

			// After line 3: one activity with partial description
			Assert.NotNull(models[2]);
			Assert.Single(models[2]!.Days![0].Activities!);
			Assert.Equal("Hello", models[2]!.Days![0].Activities![0].Description);

			// After line 4: complete activity
			Assert.NotNull(models[3]);
			Assert.Equal("Hello World", models[3]!.Days![0].Activities![0].Description);
			Assert.Equal("Activity 1", models[3]!.Days![0].Activities![0].Title);

			// After flush: same as line 4
			Assert.NotNull(models[4]);
			Assert.Equal("Hello World", models[4]!.Days![0].Activities![0].Description);
			Assert.Equal("Activity 1", models[4]!.Days![0].Activities![0].Title);
		}

		[Fact]
		public void Roundtrip_MultipleArrayItems_DeserializesCorrectly()
		{
			var lines = new[]
			{
				"""{"days": [{"subtitle": "Day 1"}]}""",
				"""{"days": [{"subtitle": "Day 1"}, {"subtitle": "Day 2"}]}"""
			};

			var models = ProcessLines<Itinerary>(lines);

			Assert.Equal(3, models.Count);

			// After line 1: one day
			Assert.NotNull(models[0]);
			Assert.Single(models[0]!.Days!);
			Assert.Equal("Day 1", models[0]!.Days![0].Subtitle);

			// After line 2: two days
			Assert.NotNull(models[1]);
			Assert.Equal(2, models[1]!.Days!.Count);
			Assert.Equal("Day 1", models[1]!.Days![0].Subtitle);
			Assert.Equal("Day 2", models[1]!.Days![1].Subtitle);
		}
	}
}
