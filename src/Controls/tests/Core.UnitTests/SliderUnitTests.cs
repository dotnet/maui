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

		// Tests for setting Min, Max, Value in all 6 possible orders
		// Order: Min, Max, Value
		[Theory]
		[InlineData(10, 100, 50)]
		[InlineData(0, 1, 0.5)]
		[InlineData(-100, 100, 0)]
		[InlineData(50, 150, 100)]
		public void SetProperties_MinMaxValue_Order(double min, double max, double value)
		{
			var slider = new Slider();
			slider.Minimum = min;
			slider.Maximum = max;
			slider.Value = value;

			Assert.Equal(min, slider.Minimum);
			Assert.Equal(max, slider.Maximum);
			Assert.Equal(value, slider.Value);
		}

		// Order: Min, Value, Max
		[Theory]
		[InlineData(10, 100, 50)]
		[InlineData(0, 1, 0.5)]
		[InlineData(-100, 100, 0)]
		[InlineData(50, 150, 100)]
		public void SetProperties_MinValueMax_Order(double min, double max, double value)
		{
			var slider = new Slider();
			slider.Minimum = min;
			slider.Value = value;
			slider.Maximum = max;

			Assert.Equal(min, slider.Minimum);
			Assert.Equal(max, slider.Maximum);
			Assert.Equal(value, slider.Value);
		}

		// Order: Max, Min, Value
		[Theory]
		[InlineData(10, 100, 50)]
		[InlineData(0, 1, 0.5)]
		[InlineData(-100, 100, 0)]
		[InlineData(50, 150, 100)]
		public void SetProperties_MaxMinValue_Order(double min, double max, double value)
		{
			var slider = new Slider();
			slider.Maximum = max;
			slider.Minimum = min;
			slider.Value = value;

			Assert.Equal(min, slider.Minimum);
			Assert.Equal(max, slider.Maximum);
			Assert.Equal(value, slider.Value);
		}

		// Order: Max, Value, Min
		[Theory]
		[InlineData(10, 100, 50)]
		[InlineData(0, 1, 0.5)]
		[InlineData(-100, 100, 0)]
		[InlineData(50, 150, 100)]
		public void SetProperties_MaxValueMin_Order(double min, double max, double value)
		{
			var slider = new Slider();
			slider.Maximum = max;
			slider.Value = value;
			slider.Minimum = min;

			Assert.Equal(min, slider.Minimum);
			Assert.Equal(max, slider.Maximum);
			Assert.Equal(value, slider.Value);
		}

		// Order: Value, Min, Max
		[Theory]
		[InlineData(10, 100, 50)]
		[InlineData(0, 1, 0.5)]
		[InlineData(-100, 100, 0)]
		[InlineData(50, 150, 100)]
		public void SetProperties_ValueMinMax_Order(double min, double max, double value)
		{
			var slider = new Slider();
			slider.Value = value;
			slider.Minimum = min;
			slider.Maximum = max;

			Assert.Equal(min, slider.Minimum);
			Assert.Equal(max, slider.Maximum);
			Assert.Equal(value, slider.Value);
		}

		// Order: Value, Max, Min
		[Theory]
		[InlineData(10, 100, 50)]
		[InlineData(0, 1, 0.5)]
		[InlineData(-100, 100, 0)]
		[InlineData(50, 150, 100)]
		public void SetProperties_ValueMaxMin_Order(double min, double max, double value)
		{
			var slider = new Slider();
			slider.Value = value;
			slider.Maximum = max;
			slider.Minimum = min;

			Assert.Equal(min, slider.Minimum);
			Assert.Equal(max, slider.Maximum);
			Assert.Equal(value, slider.Value);
		}

		// Tests that _requestedValue is preserved across multiple recoercions
		[Fact]
		public void RequestedValuePreservedAcrossMultipleRangeChanges()
		{
			var slider = new Slider();
			slider.Value = 50;
			slider.Minimum = -10;
			slider.Maximum = -1; // Value clamped to -1

			Assert.Equal(-1, slider.Value);

			slider.Maximum = -2; // Value should still be clamped, not corrupted

			Assert.Equal(-2, slider.Value);

			slider.Maximum = 100; // Now the original requested value (50) should be restored

			Assert.Equal(50, slider.Value);
		}

		[Fact]
		public void RequestedValuePreservedWhenMinimumChangesMultipleTimes()
		{
			var slider = new Slider();
			slider.Value = 5;
			slider.Maximum = 100;
			slider.Minimum = 10; // Value clamped to 10

			Assert.Equal(10, slider.Value);

			slider.Minimum = 20; // Value clamped to 20

			Assert.Equal(20, slider.Value);

			slider.Minimum = 0; // Original requested value (5) should be restored

			Assert.Equal(5, slider.Value);
		}

		[Fact]
		public void ValueClampedWhenOnlyRangeChanges()
		{
			var slider = new Slider(); // Value defaults to 0
			slider.Minimum = 10; // Value should clamp to 10
			slider.Maximum = 100;

			Assert.Equal(10, slider.Value);

			slider.Minimum = 5; // Value stays at 10 because 10 is within [5, 100]

			Assert.Equal(10, slider.Value);

			slider.Minimum = 15; // Value clamps to 15

			Assert.Equal(15, slider.Value);
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
