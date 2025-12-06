using Maui.Controls.Sample.Services;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

public partial class StreamingJsonDeserializerTests
{
	public class PartialTrailingValues
	{
		// Tests for trailing numbers that could still grow

		[Fact]
		public void ProcessChunk_TrailingNumber_StaysComplete()
		{
			var deserializer = new StreamingJsonDeserializer<NumericModel>();

			// First chunk: number 12 at end, no closing brace
			var result1 = deserializer.ProcessChunk(@"{""integer"":12");
			Assert.NotNull(result1);
			Assert.Equal(12, result1.Integer);

			// Next chunk: complete, number stays 12
			var result2 = deserializer.ProcessChunk(@"}");
			Assert.NotNull(result2);
			Assert.Equal(12, result2.Integer);
		}

		[Fact]
		public void ProcessChunk_TrailingNumber_GrowsToLargerNumber()
		{
			var deserializer = new StreamingJsonDeserializer<NumericModel>();

			// First chunk: number 12 at end, could become 1234
			var result1 = deserializer.ProcessChunk(@"{""integer"":12");
			Assert.NotNull(result1);
			Assert.Equal(12, result1.Integer);

			// Next chunk: number grows to 1234
			var result2 = deserializer.ProcessChunk(@"34}");
			Assert.NotNull(result2);
			Assert.Equal(1234, result2.Integer);
		}

		[Fact]
		public void ProcessChunk_TrailingDecimal_StaysComplete()
		{
			var deserializer = new StreamingJsonDeserializer<NumericModel>();

			// First chunk: decimal 3.14 at end
			var result1 = deserializer.ProcessChunk(@"{""decimal"":3.14");
			Assert.NotNull(result1);
			Assert.Equal(3.14, result1.Decimal);

			// Next chunk: complete, decimal stays 3.14
			var result2 = deserializer.ProcessChunk(@"}");
			Assert.NotNull(result2);
			Assert.Equal(3.14, result2.Decimal);
		}

		[Fact]
		public void ProcessChunk_TrailingDecimal_GrowsMoreDigits()
		{
			var deserializer = new StreamingJsonDeserializer<NumericModel>();

			// First chunk: decimal 3.14 at end
			var result1 = deserializer.ProcessChunk(@"{""decimal"":3.14");
			Assert.NotNull(result1);
			Assert.Equal(3.14, result1.Decimal);

			// Next chunk: decimal grows to 3.14159
			var result2 = deserializer.ProcessChunk(@"159}");
			Assert.NotNull(result2);
			Assert.Equal(3.14159, result2.Decimal);
		}

		[Fact]
		public void ProcessChunk_TrailingNegativeNumber_GrowsToLargerNumber()
		{
			var deserializer = new StreamingJsonDeserializer<NumericModel>();

			// First chunk: negative number -5 at end
			var result1 = deserializer.ProcessChunk(@"{""integer"":-5");
			Assert.NotNull(result1);
			Assert.Equal(-5, result1.Integer);

			// Next chunk: number grows to -567
			var result2 = deserializer.ProcessChunk(@"67}");
			Assert.NotNull(result2);
			Assert.Equal(-567, result2.Integer);
		}

		// Tests for trailing booleans that could still be incomplete

		[Fact]
		public void ProcessChunk_TrailingTrue_Complete()
		{
			var deserializer = new StreamingJsonDeserializer<NumericModel>();

			// First chunk: true is complete at end
			var result1 = deserializer.ProcessChunk(@"{""isActive"":true");
			Assert.NotNull(result1);
			Assert.True(result1.IsActive);

			// Next chunk: just closing
			var result2 = deserializer.ProcessChunk(@"}");
			Assert.NotNull(result2);
			Assert.True(result2.IsActive);
		}

		[Fact]
		public void ProcessChunk_TrailingFalse_Complete()
		{
			var deserializer = new StreamingJsonDeserializer<NumericModel>();

			// First chunk: false is complete at end
			var result1 = deserializer.ProcessChunk(@"{""isActive"":false");
			Assert.NotNull(result1);
			Assert.False(result1.IsActive);

			// Next chunk: just closing
			var result2 = deserializer.ProcessChunk(@"}");
			Assert.NotNull(result2);
			Assert.False(result2.IsActive);
		}

		[Fact]
		public void ProcessChunk_PartialTrue_BecomesComplete()
		{
			var deserializer = new StreamingJsonDeserializer<NumericModel>();

			// First chunk: "tru" at end - incomplete true
			var result1 = deserializer.ProcessChunk(@"{""isActive"":tru");
			Assert.NotNull(result1);
			// Value should be default (false) since "tru" is not valid
			Assert.False(result1.IsActive);

			// Next chunk: completes to "true"
			var result2 = deserializer.ProcessChunk(@"e}");
			Assert.NotNull(result2);
			Assert.True(result2.IsActive);
		}

		[Fact]
		public void ProcessChunk_PartialFalse_BecomesComplete()
		{
			var deserializer = new StreamingJsonDeserializer<NumericModel>();

			// First chunk: "fal" at end - incomplete false
			var result1 = deserializer.ProcessChunk(@"{""isActive"":fal");
			Assert.NotNull(result1);
			// Value should be default (false) since "fal" is not valid JSON
			Assert.False(result1.IsActive);

			// Next chunk: completes to "false"
			var result2 = deserializer.ProcessChunk(@"se}");
			Assert.NotNull(result2);
			Assert.False(result2.IsActive);
		}

		// Tests for trailing null that could still be incomplete

		[Fact]
		public void ProcessChunk_TrailingNull_Complete()
		{
			var deserializer = new StreamingJsonDeserializer<SimpleModel>();

			// First chunk: null is complete at end
			var result1 = deserializer.ProcessChunk(@"{""text"":null");
			Assert.NotNull(result1);
			Assert.Null(result1.Text);

			// Next chunk: just closing
			var result2 = deserializer.ProcessChunk(@"}");
			Assert.NotNull(result2);
			Assert.Null(result2.Text);
		}

		[Fact]
		public void ProcessChunk_PartialNull_BecomesComplete()
		{
			var deserializer = new StreamingJsonDeserializer<SimpleModel>();

			// First chunk: "nul" at end - incomplete null
			var result1 = deserializer.ProcessChunk(@"{""text"":nul");
			Assert.NotNull(result1);
			// Value should be default (null) since "nul" would cause parsing to skip

			// Next chunk: completes to "null"
			var result2 = deserializer.ProcessChunk(@"l}");
			Assert.NotNull(result2);
			Assert.Null(result2.Text);
		}

		// Tests for multiple trailing values

		[Fact]
		public void ProcessChunk_MultipleNumbersInProgression()
		{
			var deserializer = new StreamingJsonDeserializer<NumericModel>();

			// First chunk: first number
			var result1 = deserializer.ProcessChunk(@"{""integer"":1");
			Assert.NotNull(result1);
			Assert.Equal(1, result1.Integer);

			// Second chunk: first number grows, second appears
			var result2 = deserializer.ProcessChunk(@"00,""decimal"":3.1");
			Assert.NotNull(result2);
			Assert.Equal(100, result2.Integer);
			Assert.Equal(3.1, result2.Decimal);

			// Third chunk: second number grows
			var result3 = deserializer.ProcessChunk(@"4159}");
			Assert.NotNull(result3);
			Assert.Equal(100, result3.Integer);
			Assert.Equal(3.14159, result3.Decimal);
		}

		[Fact]
		public void ProcessChunk_NumberFollowedByBool()
		{
			var deserializer = new StreamingJsonDeserializer<NumericModel>();

			// First chunk: number at end
			var result1 = deserializer.ProcessChunk(@"{""integer"":42");
			Assert.NotNull(result1);
			Assert.Equal(42, result1.Integer);

			// Second chunk: number complete, bool appears
			var result2 = deserializer.ProcessChunk(@",""isActive"":true}");
			Assert.NotNull(result2);
			Assert.Equal(42, result2.Integer);
			Assert.True(result2.IsActive);
		}

		[Fact]
		public void ProcessChunk_NumberWithExponent_Grows()
		{
			var deserializer = new StreamingJsonDeserializer<NumericModel>();

			// First chunk: number with exponent at end
			var result1 = deserializer.ProcessChunk(@"{""decimal"":1.5e");
			Assert.NotNull(result1);
			// Incomplete exponent - may parse as 1.5 or fail gracefully

			// Second chunk: exponent completes
			var result2 = deserializer.ProcessChunk(@"2}");
			Assert.NotNull(result2);
			Assert.Equal(150, result2.Decimal); // 1.5e2 = 150
		}
	}
}
