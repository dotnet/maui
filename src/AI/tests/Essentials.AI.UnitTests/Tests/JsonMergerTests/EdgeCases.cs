using Maui.Controls.Sample.Services;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

public partial class JsonMergerTests
{
	public class EdgeCases
	{
		[Fact]
		public void MergeOverlay_NullValues_HandledCorrectly()
		{
			var baseJson = @"{""name"": ""John"", ""nickname"": null}";
			var overlay = @"{""name"": null}";
			var merger = new JsonMerger(baseJson);

			var result = merger.MergeOverlay(overlay);

			// Null from overlay should replace non-null from base
			Assert.Contains(@"""name"":null", result.Replace(" ", "", StringComparison.Ordinal), StringComparison.Ordinal);
		}

		[Fact]
		public void MergeOverlay_BooleanValues_MergedCorrectly()
		{
			var baseJson = @"{""active"": true, ""verified"": false}";
			var overlay = @"{""active"": false}";
			var merger = new JsonMerger(baseJson);

			var result = merger.MergeOverlay(overlay);

			Assert.Contains(@"""active"":false", result.Replace(" ", "", StringComparison.Ordinal), StringComparison.Ordinal);
			Assert.Contains(@"""verified"":false", result.Replace(" ", "", StringComparison.Ordinal), StringComparison.Ordinal);
		}

		[Fact]
		public void MergeOverlay_NumericValues_MergedCorrectly()
		{
			var baseJson = @"{""integer"": 42, ""decimal"": 3.14, ""negative"": -10}";
			var overlay = @"{""integer"": 100, ""decimal"": 2.71}";
			var merger = new JsonMerger(baseJson);

			var result = merger.MergeOverlay(overlay);

			Assert.Contains(@"""integer"":100", result.Replace(" ", "", StringComparison.Ordinal), StringComparison.Ordinal);
			Assert.Contains(@"""decimal"":2.71", result.Replace(" ", "", StringComparison.Ordinal), StringComparison.Ordinal);
			Assert.Contains(@"""negative"":-10", result.Replace(" ", "", StringComparison.Ordinal), StringComparison.Ordinal);
		}

		[Fact]
		public void MergeOverlay_DifferentTypes_OverlayWins()
		{
			var baseJson = @"{""value"": ""string""}";
			var overlay = @"{""value"": 123}";
			var merger = new JsonMerger(baseJson);

			var result = merger.MergeOverlay(overlay);

			// Number should replace string
			Assert.Contains(@"""value"":123", result.Replace(" ", "", StringComparison.Ordinal), StringComparison.Ordinal);
		}

		[Fact]
		public void MergeOverlay_ObjectToArray_OverlayWins()
		{
			var baseJson = @"{""data"": {""key"": ""value""}}";
			var overlay = @"{""data"": [1, 2, 3]}";
			var merger = new JsonMerger(baseJson);

			var result = merger.MergeOverlay(overlay);

			Assert.Contains(@"""data"":[1,2,3]", result.Replace(" ", "", StringComparison.Ordinal), StringComparison.Ordinal);
		}

		[Fact]
		public void MergeOverlay_UnicodeCharacters_PreservedCorrectly()
		{
			var baseJson = @"{""name"": ""Hello""}";
			var overlay = @"{""name"": ""こんにちは""}";
			var merger = new JsonMerger(baseJson);

			var result = merger.MergeOverlay(overlay);
			var parsed = merger.Deserialize<SimpleModel>(SerializerOptions);

			// Verify via deserialization to handle JSON escaping
			Assert.NotNull(parsed);
			Assert.Equal("こんにちは", parsed.Name);
		}

		[Fact]
		public void MergeOverlay_EscapedCharacters_HandledCorrectly()
		{
			var baseJson = @"{""text"": ""Line1\nLine2""}";
			var overlay = @"{""text"": ""Ligne1\nLigne2""}";
			var merger = new JsonMerger(baseJson);

			var result = merger.MergeOverlay(overlay);

			Assert.Contains("Ligne1", result, StringComparison.Ordinal);
			Assert.Contains("Ligne2", result, StringComparison.Ordinal);
		}

		[Fact]
		public void MergeOverlay_EmptyObject_PreservesBase()
		{
			var baseJson = @"{""name"": ""John"", ""age"": 30}";
			var overlay = @"{}";
			var merger = new JsonMerger(baseJson);

			var result = merger.MergeOverlay(overlay);
			var parsed = merger.Deserialize<SimpleModel>(SerializerOptions);

			Assert.NotNull(parsed);
			Assert.Equal("John", parsed.Name);
			Assert.Equal(30, parsed.Age);
		}

		[Fact]
		public void MergeOverlay_WhitespaceInJson_HandledCorrectly()
		{
			var baseJson = @"  {  ""name""  :  ""John""  }  ";
			var overlay = @"  {  ""name""  :  ""Jane""  }  ";
			var merger = new JsonMerger(baseJson);

			var result = merger.MergeOverlay(overlay);
			var parsed = merger.Deserialize<SimpleModel>(SerializerOptions);

			Assert.Equal("Jane", parsed?.Name);
		}

		[Fact]
		public void MergeOverlayUtf8_WorksWithByteSpans()
		{
			var baseJson = @"{""name"": ""John""}";
			var overlayBytes = System.Text.Encoding.UTF8.GetBytes(@"{""name"": ""Jane""}");
			var merger = new JsonMerger(baseJson);

			merger.MergeOverlayUtf8(overlayBytes);

			Assert.Contains("Jane", merger.Result, StringComparison.Ordinal);
		}
	}
}
