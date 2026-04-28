using Maui.Controls.Sample.Services;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

public partial class JsonMergerTests
{
	public class ArrayMerging
	{
		[Fact]
		public void MergeOverlay_Array_MergesByIndex()
		{
			var baseJson = @"{
				""title"": ""Shopping List"",
				""items"": [
					{""name"": ""Apple"", ""description"": ""Red fruit"", ""price"": 1.50},
					{""name"": ""Banana"", ""description"": ""Yellow fruit"", ""price"": 0.75}
				]
			}";
			var overlay = @"{
				""items"": [
					{""name"": ""Pomme"", ""description"": ""Fruit rouge""}
				]
			}";
			var merger = new JsonMerger(baseJson);

			var result = merger.MergeOverlay(overlay);
			var parsed = merger.Deserialize<ArrayModel>(SerializerOptions);

			Assert.NotNull(parsed);
			Assert.Equal("Shopping List", parsed.Title);  // Base preserved
			Assert.NotNull(parsed.Items);
			Assert.Equal(2, parsed.Items.Count);          // Both items preserved

			// First item merged
			Assert.Equal("Pomme", parsed.Items[0].Name);               // Overlay
			Assert.Equal("Fruit rouge", parsed.Items[0].Description);  // Overlay
			Assert.Equal(1.50m, parsed.Items[0].Price);                // Base preserved

			// Second item unchanged
			Assert.Equal("Banana", parsed.Items[1].Name);
			Assert.Equal("Yellow fruit", parsed.Items[1].Description);
			Assert.Equal(0.75m, parsed.Items[1].Price);
		}

		[Fact]
		public void MergeOverlay_Array_OverlayLongerThanBase()
		{
			var baseJson = @"{""items"": [{""name"": ""A""}]}";
			var overlay = @"{""items"": [{""name"": ""B""}, {""name"": ""C""}]}";
			var merger = new JsonMerger(baseJson);

			var result = merger.MergeOverlay(overlay);
			var parsed = merger.Deserialize<ArrayModel>(SerializerOptions);

			Assert.NotNull(parsed?.Items);
			Assert.Equal(2, parsed.Items.Count);
			Assert.Equal("B", parsed.Items[0].Name);  // Overlay replaced
			Assert.Equal("C", parsed.Items[1].Name);  // Overlay added
		}

		[Fact]
		public void MergeOverlay_Array_BaseLongerThanOverlay()
		{
			var baseJson = @"{""items"": [{""name"": ""A""}, {""name"": ""B""}, {""name"": ""C""}]}";
			var overlay = @"{""items"": [{""name"": ""X""}]}";
			var merger = new JsonMerger(baseJson);

			var result = merger.MergeOverlay(overlay);
			var parsed = merger.Deserialize<ArrayModel>(SerializerOptions);

			Assert.NotNull(parsed?.Items);
			Assert.Equal(3, parsed.Items.Count);
			Assert.Equal("X", parsed.Items[0].Name);  // Overlay replaced
			Assert.Equal("B", parsed.Items[1].Name);  // Base preserved
			Assert.Equal("C", parsed.Items[2].Name);  // Base preserved
		}

		[Fact]
		public void MergeOverlay_EmptyArrayOverlay_PreservesBase()
		{
			var baseJson = @"{""items"": [{""name"": ""A""}, {""name"": ""B""}]}";
			var overlay = @"{""items"": []}";
			var merger = new JsonMerger(baseJson);

			var result = merger.MergeOverlay(overlay);
			var parsed = merger.Deserialize<ArrayModel>(SerializerOptions);

			Assert.NotNull(parsed?.Items);
			// Empty overlay array preserves base items (useful for partial translation scenarios)
			Assert.Equal(2, parsed.Items.Count);
			Assert.Equal("A", parsed.Items[0].Name);
			Assert.Equal("B", parsed.Items[1].Name);
		}
	}
}
