using System;

using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class StepperUnitTests : BaseTestFixture
	{
		[Fact]
		public void TestConstructor()
		{
			var stepper = new Stepper(120, 200, 150, 2);

			Assert.Equal(120, stepper.Minimum);
			Assert.Equal(200, stepper.Maximum);
			Assert.Equal(150, stepper.Value);
			Assert.Equal(2, stepper.Increment);
		}

		[Fact]
		public void TestInvalidConstructor()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => new Stepper(100, 0, 50, 1));
		}

		[Fact]
		public void TestInvalidMaxValue()
		{
			Stepper stepper = new Stepper();
			stepper.Maximum = stepper.Minimum - 1;
			Assert.NotEqual(stepper.Minimum, stepper.Maximum);
		}

		[Fact]
		public void TestInvalidMinValue()
		{
			Stepper stepper = new Stepper();
			stepper.Minimum = stepper.Maximum + 1;
			Assert.NotEqual(stepper.Minimum, stepper.Maximum);
		}

		[Fact]
		public void TestValidMaxValue()
		{
			Stepper stepper = new Stepper();
			stepper.Maximum = 2000;
			Assert.Equal(2000, stepper.Maximum);
		}

		[Fact]
		public void TestValidMinValue()
		{
			Stepper stepper = new Stepper();

			stepper.Maximum = 2000;
			stepper.Minimum = 200;

			Assert.Equal(200, stepper.Minimum);
		}

		[Fact]
		public void TestConstructorClampValue()
		{
			Stepper stepper = new Stepper(0, 100, 2000, 1);

			Assert.Equal(100, stepper.Value);

			stepper = new Stepper(0, 100, -200, 1);

			Assert.Equal(0, stepper.Value);
		}

		[Fact]
		public void TestMinClampValue()
		{
			Stepper stepper = new Stepper();

			bool minThrown = false;
			bool valThrown = false;

			stepper.PropertyChanged += (sender, e) =>
			{
				switch (e.PropertyName)
				{
					case "Minimum":
						minThrown = true;
						break;
					case "Value":
						valThrown = true;
						break;
				}
			};

			stepper.Minimum = 10;

			Assert.Equal(10, stepper.Minimum);
			Assert.Equal(10, stepper.Value);
			Assert.True(minThrown);
			Assert.True(valThrown);
		}

		[Fact]
		public void TestMaxClampValue()
		{
			Stepper stepper = new Stepper();

			stepper.Value = 50;

			bool maxThrown = false;
			bool valThrown = false;

			stepper.PropertyChanged += (sender, e) =>
			{
				switch (e.PropertyName)
				{
					case "Maximum":
						maxThrown = true;
						break;
					case "Value":
						valThrown = true;
						break;
				}
			};

			stepper.Maximum = 25;

			Assert.Equal(25, stepper.Maximum);
			Assert.Equal(25, stepper.Value);
			Assert.True(maxThrown);
			Assert.True(valThrown);
		}

		[Fact]
		public void TestValueChangedEvent()
		{
			var stepper = new Stepper();

			bool fired = false;
			stepper.ValueChanged += (sender, arg) => fired = true;

			stepper.Value = 50;

			Assert.True(fired);
		}

		[Theory]
		[InlineData(100.0, 0.5)]
		[InlineData(10.0, 25.0)]
		[InlineData(0, 39.5)]
		public void StepperValueChangedEventArgs(double initialValue, double finalValue)
		{
			var stepper = new Stepper
			{
				Maximum = 100,
				Minimum = 0,
				Increment = 0.5,
				Value = initialValue
			};

			Stepper stepperFromSender = null;
			var oldValue = 0.0;
			var newValue = 0.0;

			stepper.ValueChanged += (s, e) =>
			{
				stepperFromSender = (Stepper)s;
				oldValue = e.OldValue;
				newValue = e.NewValue;
			};

			stepper.Value = finalValue;

			Assert.Equal(stepper, stepperFromSender);
			Assert.Equal(initialValue, oldValue);
			Assert.Equal(finalValue, newValue);
		}

		[Theory]
		[InlineData(10)]
		public void TestReturnToZero(int steps)
		{
			var stepper = new Stepper(0, 10, 0, 0.5);

			for (int i = steps; i < steps; i++)
				stepper.Value += stepper.Increment;

			for (int i = steps; i < steps; i--)
				stepper.Value += stepper.Increment;

			Assert.Equal(0.0, stepper.Value);
		}

		[Theory]
		[InlineData(100, .5, 0, 100)]
		[InlineData(100, .3, 0, 100)]
		[InlineData(100, .03, 0, 100)]
		[InlineData(100, .003, 0, 100)]
		[InlineData(100, .0003, 0, 100)]
		[InlineData(100, .0000003, 0, 100)]
		[InlineData(100, .0000000003, 0, 100)]
		[InlineData(100, .0000000000003, 0, 100)]
		[InlineData(100, .5, -10000, 10000)]
		[InlineData(100, .3, -10000, 10000)]
		[InlineData(100, .03, -10000, 10000)]
		[InlineData(100, .003, -10000, 10000)]
		[InlineData(100, .0003, -10000, 10000)]
		[InlineData(100, .0000003, -10000, 10000)]
		[InlineData(100, .0000000003, -10000, 10000)]
		[InlineData(100, .0000000000003, -10000, 10000)]
		[InlineData(100, .00003456, -10000, 10000)] //we support 4 significant digits for the increment. no less, no more
													//https://github.com/xamarin/Microsoft.Maui.Controls/issues/5168
		public void SmallIncrements(int steps, double increment, double min, double max)
		{
			var stepper = new Stepper(min, max, 0, increment);
			int digits = Math.Max(1, Math.Min(15, (int)(-Math.Log10((double)increment) + 4))); //logic copied from the Stepper code

			Assert.Equal(0.0, stepper.Value);

			for (var i = 0; i < steps; i++)
				stepper.Value += stepper.Increment;

			Assert.Equal(Math.Round(stepper.Increment * steps, digits), stepper.Value);

			for (var i = 0; i < steps; i++)
				stepper.Value -= stepper.Increment;

			Assert.Equal(0.0, stepper.Value);
		}

		[Fact]
		//https://github.com/xamarin/Microsoft.Maui.Controls/issues/10032
		public void InitialValue()
		{
			var increment = .1;
			var stepper = new Stepper(0, 10, 4.99, increment);

			Assert.Equal(4.99, stepper.Value);

			stepper.Value += stepper.Increment;
			Assert.Equal(5.09, stepper.Value);
			stepper.Value += stepper.Increment;
			Assert.Equal(5.19, stepper.Value);
			stepper.Value += stepper.Increment;
			Assert.Equal(5.29, stepper.Value);
			stepper.Value += stepper.Increment;
			Assert.Equal(5.39, stepper.Value);
		}

		// Tests for setting Min, Max, Value in all 6 possible orders
		// Order: Min, Max, Value
		[Theory]
		[InlineData(10, 100, 50)]
		[InlineData(0, 50, 25)]
		[InlineData(-100, 100, 0)]
		[InlineData(50, 150, 100)]
		public void SetProperties_MinMaxValue_Order(double min, double max, double value)
		{
			var stepper = new Stepper();
			stepper.Minimum = min;
			stepper.Maximum = max;
			stepper.Value = value;

			Assert.Equal(min, stepper.Minimum);
			Assert.Equal(max, stepper.Maximum);
			Assert.Equal(value, stepper.Value);
		}

		// Order: Min, Value, Max
		[Theory]
		[InlineData(10, 200, 50)]
		[InlineData(0, 50, 25)]
		[InlineData(-100, 100, 0)]
		[InlineData(50, 150, 100)]
		public void SetProperties_MinValueMax_Order(double min, double max, double value)
		{
			var stepper = new Stepper();
			stepper.Minimum = min;
			stepper.Value = value;
			stepper.Maximum = max;

			Assert.Equal(min, stepper.Minimum);
			Assert.Equal(max, stepper.Maximum);
			Assert.Equal(value, stepper.Value);
		}

		// Order: Max, Min, Value
		[Theory]
		[InlineData(10, 200, 50)]
		[InlineData(0, 50, 25)]
		[InlineData(-100, 100, 0)]
		[InlineData(50, 150, 100)]
		public void SetProperties_MaxMinValue_Order(double min, double max, double value)
		{
			var stepper = new Stepper();
			stepper.Maximum = max;
			stepper.Minimum = min;
			stepper.Value = value;

			Assert.Equal(min, stepper.Minimum);
			Assert.Equal(max, stepper.Maximum);
			Assert.Equal(value, stepper.Value);
		}

		// Order: Max, Value, Min
		[Theory]
		[InlineData(10, 200, 50)]
		[InlineData(0, 50, 25)]
		[InlineData(-100, 100, 0)]
		[InlineData(50, 150, 100)]
		public void SetProperties_MaxValueMin_Order(double min, double max, double value)
		{
			var stepper = new Stepper();
			stepper.Maximum = max;
			stepper.Value = value;
			stepper.Minimum = min;

			Assert.Equal(min, stepper.Minimum);
			Assert.Equal(max, stepper.Maximum);
			Assert.Equal(value, stepper.Value);
		}

		// Order: Value, Min, Max
		[Theory]
		[InlineData(10, 200, 50)]
		[InlineData(0, 50, 25)]
		[InlineData(-100, 100, 0)]
		[InlineData(50, 150, 100)]
		public void SetProperties_ValueMinMax_Order(double min, double max, double value)
		{
			var stepper = new Stepper();
			stepper.Value = value;
			stepper.Minimum = min;
			stepper.Maximum = max;

			Assert.Equal(min, stepper.Minimum);
			Assert.Equal(max, stepper.Maximum);
			Assert.Equal(value, stepper.Value);
		}

		// Order: Value, Max, Min
		[Theory]
		[InlineData(10, 200, 50)]
		[InlineData(0, 50, 25)]
		[InlineData(-100, 100, 0)]
		[InlineData(50, 150, 100)]
		public void SetProperties_ValueMaxMin_Order(double min, double max, double value)
		{
			var stepper = new Stepper();
			stepper.Value = value;
			stepper.Maximum = max;
			stepper.Minimum = min;

			Assert.Equal(min, stepper.Minimum);
			Assert.Equal(max, stepper.Maximum);
			Assert.Equal(value, stepper.Value);
		}

		// Tests that _requestedValue is preserved across multiple recoercions
		[Fact]
		public void RequestedValuePreservedAcrossMultipleRangeChanges()
		{
			var stepper = new Stepper();
			stepper.Value = 50;
			stepper.Minimum = -10;
			stepper.Maximum = -1; // Value clamped to -1

			Assert.Equal(-1, stepper.Value);

			stepper.Maximum = -2; // Value should still be clamped, not corrupted

			Assert.Equal(-2, stepper.Value);

			stepper.Maximum = 100; // Now the original requested value (50) should be restored

			Assert.Equal(50, stepper.Value);
		}

		[Fact]
		public void RequestedValuePreservedWhenMinimumChangesMultipleTimes()
		{
			var stepper = new Stepper();
			stepper.Value = 5;
			stepper.Maximum = 100;
			stepper.Minimum = 10; // Value clamped to 10

			Assert.Equal(10, stepper.Value);

			stepper.Minimum = 20; // Value clamped to 20

			Assert.Equal(20, stepper.Value);

			stepper.Minimum = 0; // Original requested value (5) should be restored

			Assert.Equal(5, stepper.Value);
		}

		[Fact]
		public void ValueClampedWhenOnlyRangeChanges()
		{
			var stepper = new Stepper(); // Value defaults to 0
			stepper.Minimum = 10; // Value should clamp to 10

			Assert.Equal(10, stepper.Value);

			stepper.Minimum = 5; // Value stays at 10 because 10 is within [5, 100]

			Assert.Equal(10, stepper.Value);

			stepper.Minimum = 15; // Value clamps to 15

			Assert.Equal(15, stepper.Value);
		}
	}
}
