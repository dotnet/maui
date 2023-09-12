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
						Assert.False(minThrown);
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
						Assert.False(maxThrown);
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
	}
}
