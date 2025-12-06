using Maui.Controls.Sample.Services;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

public partial class StreamingJsonDeserializerTests
{
	public class DataTypes
	{
		[Fact]
		public void ProcessChunk_NestedObject_DeserializesIncrementally()
		{
			var deserializer = new StreamingJsonDeserializer<NestedModel>();

			var result1 = deserializer.ProcessChunk(@"{""text"": ""Hello"", ""meta"": {""score"": 1, ""author"": ""John""}}");
			Assert.NotNull(result1);
			Assert.Equal("Hello", result1.Text);
			Assert.NotNull(result1.Meta);
			Assert.Equal(1, result1.Meta.Score);
			Assert.Equal("John", result1.Meta.Author);
			
			// Now test incremental addition
			deserializer.Reset();
			var result2 = deserializer.ProcessChunk(@"{""text"": ""Hi""}");
			Assert.NotNull(result2);
			Assert.Equal("Hi", result2.Text);
		}

		[Fact]
		public void ProcessChunk_ArrayIncrementalParsing_UpdatesCorrectly()
		{
			var deserializer = new StreamingJsonDeserializer<ComplexModel>();

			var result1 = deserializer.ProcessChunk(@"{""title"": ""Store"", ""items"": [{""name"": ""Laptop"", ""price"": 999.99, ""category"": ""Electronics""}");
			Assert.NotNull(result1);
			Assert.Equal("Store", result1.Title);
			Assert.NotNull(result1.Items);
			Assert.Single(result1.Items);
			Assert.Equal("Laptop", result1.Items[0].Name);
			Assert.Equal(999.99m, result1.Items[0].Price);
			Assert.Equal(ItemCategory.Electronics, result1.Items[0].Category);

			var result2 = deserializer.ProcessChunk(@", {""name"": ""Shirt"", ""price"": 29.99, ""category"": ""Clothing""}]}");
			Assert.NotNull(result2);
			Assert.NotNull(result2.Items);
			Assert.Equal(2, result2.Items.Count);
			Assert.Equal("Shirt", result2.Items[1].Name);
			Assert.Equal(ItemCategory.Clothing, result2.Items[1].Category);
		}

		[Fact]
		public void ProcessChunk_IncompleteArray_ReturnsPartialData()
		{
			var deserializer = new StreamingJsonDeserializer<ComplexModel>();

			var result = deserializer.ProcessChunk(@"{""title"": ""Inventory"", ""items"": [{""name"": ""Item1"", ""price"": 10, ""category"": ""Food""}");

			Assert.NotNull(result);
			Assert.Equal("Inventory", result.Title);
			Assert.NotNull(result.Items);
			Assert.Single(result.Items);
			Assert.Equal("Item1", result.Items[0].Name);
			Assert.Equal(10m, result.Items[0].Price);
		}

		[Fact]
		public void ProcessChunk_NumbersAndBooleans_ParseCorrectly()
		{
			var deserializer = new StreamingJsonDeserializer<NumericModel>();

			var result1 = deserializer.ProcessChunk(@"{""integer"": 42, ""decimal"": 3.14, ""isActive"": true, ""isComplete"": false}");
			Assert.NotNull(result1);
			Assert.Equal(42, result1.Integer);
			Assert.Equal(3.14, result1.Decimal);
			Assert.True(result1.IsActive);
			Assert.False(result1.IsComplete);
		}

		[Fact]
		public void ProcessChunk_NullValues_HandledCorrectly()
		{
			var deserializer = new StreamingJsonDeserializer<SimpleModel>();

			var result = deserializer.ProcessChunk(@"{""text"": null, ""score"": 0}");

			Assert.NotNull(result);
			Assert.Null(result.Text);
			Assert.Equal(0, result.Score);
		}
	}
}
