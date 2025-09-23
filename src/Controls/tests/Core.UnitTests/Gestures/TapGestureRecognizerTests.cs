using System;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class TapGestureRecognizerTests : BaseTestFixture
	{
		[Fact]
		public void Constructor()
		{
			var tap = new TapGestureRecognizer();

			Assert.Null(tap.Command);
			Assert.Null(tap.CommandParameter);
			Assert.Equal(1, tap.NumberOfTapsRequired);
		}

		[Fact]
		public void CallbackPassesParameter()
		{
			var view = new View();
			var tap = new TapGestureRecognizer();
			tap.CommandParameter = "Hello";

			object result = null;
			tap.Command = new Command(o => result = o);

			tap.SendTapped(view);
			Assert.Equal(result, tap.CommandParameter);
		}

		[Theory]
		[InlineData(1)]
		[InlineData(2)]
		[InlineData(3)]
		[InlineData(4)]
		[InlineData(5)]
		[InlineData(10)]
		public void SupportsMultipleTapsRequired(int numberOfTaps)
		{
			var tap = new TapGestureRecognizer
			{
				NumberOfTapsRequired = numberOfTaps
			};

			Assert.Equal(numberOfTaps, tap.NumberOfTapsRequired);
		}

		[Fact]
		public void MultiTapGestureCanBeTriggered()
		{
			var view = new View();
			var tap = new TapGestureRecognizer
			{
				NumberOfTapsRequired = 3
			};

			int tappedCount = 0;
			tap.Command = new Command(() => tappedCount++);

			tap.SendTapped(view);
			Assert.Equal(1, tappedCount);
		}
	}
}
