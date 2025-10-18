using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class SliderUnitTests : BaseTestFixture
	{
		[Fact]
		public void TestConstructor()
		{
			var slider = new Slider(20, 200, 50);

			Assert.Equal(20, slider.Minimum);
			Assert.Equal(200, slider.Maximum);
			Assert.Equal(50, slider.Value);
		}

		[Fact]
		public void TestInvalidConstructor()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => new Slider(10, 5, 10));
		}

		[Fact]
		public void TestConstructorClamping()
		{
			Slider slider = new Slider(50, 100, 0);

			Assert.Equal(50, slider.Value);
		}

		[Fact]
		public void TestMinValueClamp()
		{
			Slider slider = new Slider(0, 100, 0);

			slider.Minimum = 10;

			Assert.Equal(10, slider.Value);
			Assert.Equal(10, slider.Minimum);
		}

		[Fact]
		public void TestMaxValueClamp()
		{
			Slider slider = new Slider(0, 100, 100);

			slider.Maximum = 10;

			Assert.Equal(10, slider.Value);
			Assert.Equal(10, slider.Maximum);
		}

		[Fact]
		public void TestInvalidMaxValue()
		{
			var slider = new Slider();
			slider.Maximum = slider.Minimum - 1;
		}

		[Fact]
		public void TestInvalidMinValue()
		{
			var slider = new Slider();
			slider.Minimum = slider.Maximum + 1;
		}

		[Fact]
		public void TestValueChanged()
		{
			var slider = new Slider();
			var changed = false;

			slider.ValueChanged += (sender, arg) => changed = true;

			slider.Value += 1;

			Assert.True(changed);
		}

		[Theory]
		[InlineData(0.0, 1.0)]
		[InlineData(1.0, 0.5)]
		public void SliderValueChangedEventArgs(double initialValue, double finalValue)
		{
			var slider = new Slider
			{
				Minimum = 0.0,
				Maximum = 1.0,
				Value = initialValue
			};

			Slider sliderFromSender = null;
			double oldValue = 0.0;
			double newValue = 0.0;

			slider.ValueChanged += (s, e) =>
			{
				sliderFromSender = (Slider)s;
				oldValue = e.OldValue;
				newValue = e.NewValue;
			};

			slider.Value = finalValue;

			Assert.Equal(slider, sliderFromSender);
			Assert.Equal(initialValue, oldValue);
			Assert.Equal(finalValue, newValue);
		}

		[Fact]
		public void TestDragStarted()
		{
			var slider = new Slider();
			var started = false;

			slider.DragStarted += (sender, arg) => started = true;
			((ISliderController)slider).SendDragStarted();

			Assert.True(started);
		}

		[Fact]
		public void TestDragCompleted()
		{
			var slider = new Slider();
			var completed = false;

			slider.DragCompleted += (sender, arg) => completed = true;
			((ISliderController)slider).SendDragCompleted();

			Assert.True(completed);
		}
	}
}
