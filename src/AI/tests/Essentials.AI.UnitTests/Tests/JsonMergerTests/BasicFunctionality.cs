using System.Text;
using System.Text.Json;
using Maui.Controls.Sample.Services;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

public partial class JsonMergerTests
{
	public class BasicFunctionality
	{
		[Fact]
		public void MergeOverlay_EmptyOverlay_ReturnsBase()
		{
			var baseJson = @"{""name"": ""John""}";
			var merger = new JsonMerger(baseJson);

			var result = merger.MergeOverlay("");

			// Compare semantically - whitespace may differ but content should be the same
			using var expectedDoc = JsonDocument.Parse(baseJson);
			using var actualDoc = JsonDocument.Parse(result);
			Assert.Equal(
				expectedDoc.RootElement.GetProperty("name").GetString(),
				actualDoc.RootElement.GetProperty("name").GetString());
		}

		[Fact]
		public void MergeOverlay_SimpleObject_OverlayReplacesBase()
		{
			var baseJson = @"{""name"": ""John"", ""age"": 30}";
			var overlay = @"{""name"": ""Jane""}";
			var merger = new JsonMerger(baseJson);

			var result = merger.MergeOverlay(overlay);
			var parsed = merger.Deserialize<SimpleModel>(SerializerOptions);

			Assert.NotNull(parsed);
			Assert.Equal("Jane", parsed.Name);  // Overlay replaced
			Assert.Equal(30, parsed.Age);       // Base preserved
		}

		[Fact]
		public void MergeOverlay_OverlayAddsNewProperty()
		{
			var baseJson = @"{""name"": ""John""}";
			var overlay = @"{""age"": 25}";
			var merger = new JsonMerger(baseJson);

			var result = merger.MergeOverlay(overlay);
			var parsed = merger.Deserialize<SimpleModel>(SerializerOptions);

			Assert.NotNull(parsed);
			Assert.Equal("John", parsed.Name);  // Base preserved
			Assert.Equal(25, parsed.Age);       // Overlay added
		}

		[Fact]
		public void MergeOverlay_FullOverlay_ReplacesAllProperties()
		{
			var baseJson = @"{""name"": ""John"", ""age"": 30, ""city"": ""NYC""}";
			var overlay = @"{""name"": ""Jane"", ""age"": 25, ""city"": ""LA""}";
			var merger = new JsonMerger(baseJson);

			var result = merger.MergeOverlay(overlay);
			var parsed = merger.Deserialize<SimpleModel>(SerializerOptions);

			Assert.NotNull(parsed);
			Assert.Equal("Jane", parsed.Name);
			Assert.Equal(25, parsed.Age);
			Assert.Equal("LA", parsed.City);
		}

		[Fact]
		public void Constructor_NullOrEmptyBase_Throws()
		{
			Assert.Throws<ArgumentNullException>(() => new JsonMerger((string)null!));
			Assert.Throws<ArgumentException>(() => new JsonMerger(string.Empty));
		}
	}
}
