using System;

using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class StepperUnitTests : BaseTestFixture
	{
		[Test]
		public void TestConstructor()
		{
			var stepper = new Stepper(120, 200, 150, 2);

			Assert.AreEqual(120, stepper.Minimum);
			Assert.AreEqual(200, stepper.Maximum);
			Assert.AreEqual(150, stepper.Value);
			Assert.AreEqual(2, stepper.Increment);
		}

		[Test]
		public void TestInvalidConstructor()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => new Stepper(100, 0, 50, 1));
		}

		[Test]
		public void TestInvalidMaxValue()
		{
			Stepper stepper = new Stepper();
			Assert.Throws<ArgumentException>(() => stepper.Maximum = stepper.Minimum - 1);
		}

		[Test]
		public void TestInvalidMinValue()
		{
			Stepper stepper = new Stepper();
			Assert.Throws<ArgumentException>(() => stepper.Minimum = stepper.Maximum + 1);
		}

		[Test]
		public void TestValidMaxValue()
		{
			Stepper stepper = new Stepper();

			stepper.Maximum = 2000;

			Assert.AreEqual(2000, stepper.Maximum);
		}

		[Test]
		public void TestValidMinValue()
		{
			Stepper stepper = new Stepper();

			stepper.Maximum = 2000;
			stepper.Minimum = 200;

			Assert.AreEqual(200, stepper.Minimum);
		}

		[Test]
		public void TestConstructorClampValue()
		{
			Stepper stepper = new Stepper(0, 100, 2000, 1);

			Assert.AreEqual(100, stepper.Value);

			stepper = new Stepper(0, 100, -200, 1);

			Assert.AreEqual(0, stepper.Value);
		}

		[Test]
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

			Assert.AreEqual(10, stepper.Minimum);
			Assert.AreEqual(10, stepper.Value);
			Assert.True(minThrown);
			Assert.True(valThrown);
		}

		[Test]
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

			Assert.AreEqual(25, stepper.Maximum);
			Assert.AreEqual(25, stepper.Value);
			Assert.True(maxThrown);
			Assert.True(valThrown);
		}

		[Test]
		public void TestValueChangedEvent()
		{
			var stepper = new Stepper();

			bool fired = false;
			stepper.ValueChanged += (sender, arg) => fired = true;

			stepper.Value = 50;

			Assert.True(fired);
		}

		[TestCase(100.0, 0.5)]
		[TestCase(10.0, 25.0)]
		[TestCase(0, 39.5)]
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

			Assert.AreEqual(stepper, stepperFromSender);
			Assert.AreEqual(initialValue, oldValue);
			Assert.AreEqual(finalValue, newValue);
		}

		[TestCase(10)]
		public void TestReturnToZero(int steps)
		{
			var stepper = new Stepper(0, 10, 0, 0.5);

			for (int i = steps; i < steps; i++)
			{
				stepper.Value += stepper.Increment;
			}
			for (int i = steps; i < steps; i--)
			{
				stepper.Value += stepper.Increment;
			}

			Assert.AreEqual(0.0, stepper.Value);
		}
	}
}
