using Microsoft.Maui.Controls.Internals;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class BooleanBoxesTests : BaseTestFixture
	{
		[Fact]
		public void TrueBoxIsBoxedTrue()
		{
			Assert.Equal(true, BooleanBoxes.TrueBox);
			Assert.IsType<bool>(BooleanBoxes.TrueBox);
		}

		[Fact]
		public void FalseBoxIsBoxedFalse()
		{
			Assert.Equal(false, BooleanBoxes.FalseBox);
			Assert.IsType<bool>(BooleanBoxes.FalseBox);
		}

		[Fact]
		public void TrueBoxAndFalseBoxAreDifferentInstances()
		{
			Assert.NotSame(BooleanBoxes.TrueBox, BooleanBoxes.FalseBox);
		}

		[Fact]
		public void BoxTrueReturnsTrueBox()
		{
			var result = BooleanBoxes.Box(true);
			Assert.Same(BooleanBoxes.TrueBox, result);
		}

		[Fact]
		public void BoxFalseReturnsFalseBox()
		{
			var result = BooleanBoxes.Box(false);
			Assert.Same(BooleanBoxes.FalseBox, result);
		}

		[Fact]
		public void BoxNullableTrueReturnsTrueBox()
		{
			bool? value = true;
			var result = BooleanBoxes.Box(value);
			Assert.Same(BooleanBoxes.TrueBox, result);
		}

		[Fact]
		public void BoxNullableFalseReturnsFalseBox()
		{
			bool? value = false;
			var result = BooleanBoxes.Box(value);
			Assert.Same(BooleanBoxes.FalseBox, result);
		}

		[Fact]
		public void BoxNullableNullReturnsNull()
		{
			bool? value = null;
			var result = BooleanBoxes.Box(value);
			Assert.Null(result);
		}

		[Fact]
		public void BoxReturnsSameReferenceOnRepeatedCalls()
		{
			Assert.Same(BooleanBoxes.Box(true), BooleanBoxes.Box(true));
			Assert.Same(BooleanBoxes.Box(false), BooleanBoxes.Box(false));
		}
	}
}
