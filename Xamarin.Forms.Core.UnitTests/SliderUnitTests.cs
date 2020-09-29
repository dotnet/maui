using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class SliderUnitTests : BaseTestFixture
	{
		[Test]
		public void TestConstructor()
		{
			var slider = new Slider(20, 200, 50);

			Assert.AreEqual(20, slider.Minimum);
			Assert.AreEqual(200, slider.Maximum);
			Assert.AreEqual(50, slider.Value);
		}

		[Test]
		public void TestInvalidConstructor()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => new Slider(10, 5, 10));
		}

		[Test]
		public void TestConstructorClamping()
		{
			Slider slider = new Slider(50, 100, 0);

			Assert.AreEqual(50, slider.Value);
		}

		[Test]
		public void TestMinValueClamp()
		{
			Slider slider = new Slider(0, 100, 0);

			slider.Minimum = 10;

			Assert.AreEqual(10, slider.Value);
			Assert.AreEqual(10, slider.Minimum);
		}

		[Test]
		public void TestMaxValueClamp()
		{
			Slider slider = new Slider(0, 100, 100);

			slider.Maximum = 10;

			Assert.AreEqual(10, slider.Value);
			Assert.AreEqual(10, slider.Maximum);
		}

		[Test]
		public void TestInvalidMaxValue()
		{
			var slider = new Slider();
			Assert.Throws<ArgumentException>(() => slider.Maximum = slider.Minimum - 1);
		}

		[Test]
		public void TestInvalidMinValue()
		{
			var slider = new Slider();
			Assert.Throws<ArgumentException>(() => slider.Minimum = slider.Maximum + 1);
		}

		[Test]
		public void TestValueChanged()
		{
			var slider = new Slider();
			var changed = false;

			slider.ValueChanged += (sender, arg) => changed = true;

			slider.Value += 1;

			Assert.True(changed);
		}

		[TestCase(0.0, 1.0)]
		[TestCase(1.0, 0.5)]
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

			Assert.AreEqual(slider, sliderFromSender);
			Assert.AreEqual(initialValue, oldValue);
			Assert.AreEqual(finalValue, newValue);
		}

		[Test]
		public void TestDragStarted()
		{
			var slider = new Slider();
			var started = false;

			slider.DragStarted += (sender, arg) => started = true;
			((ISliderController)slider).SendDragStarted();

			Assert.True(started);
		}

		[Test]
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