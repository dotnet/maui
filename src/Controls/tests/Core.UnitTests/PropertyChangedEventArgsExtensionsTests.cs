using System.ComponentModel;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[TestFixture]
	public class PropertyChangedEventArgsExtensionsTests : BaseTestFixture
	{
		[Test]
		public void IsWithMathingStringValues()
		{
			PropertyChangedEventArgs args = new("MyProperty");
			BindableProperty bindable = BindableProperty.Create("MyProperty", typeof(object), typeof(object));

			Assert.IsTrue(args.Is(bindable));
		}

		[Test]
		public void IsWithNonMathingValues()
		{
			PropertyChangedEventArgs args = new("MyProperty");
			BindableProperty bindable = BindableProperty.Create("NotMyProperty", typeof(object), typeof(object));

			Assert.IsFalse(args.Is(bindable));
		}

		[Test]
		[TestCase("")]
		[TestCase(null)]
		public void IsWithNullOrEmptyValue(string changedPropertyName)
		{
			PropertyChangedEventArgs args = new(changedPropertyName);
			BindableProperty bindable = BindableProperty.Create("MyProperty", typeof(object), typeof(object));

			Assert.IsTrue(args.Is(bindable));
		}

		[Test]
		public void IsOneOfTwoParameters()
		{
			PropertyChangedEventArgs args = new("MyProperty");
			BindableProperty bindable0 = BindableProperty.Create("MyProperty", typeof(object), typeof(object));
			BindableProperty bindable1 = BindableProperty.Create("AlsoMyProperty", typeof(object), typeof(object));

			Assert.IsTrue(args.IsOneOf(bindable0, bindable1));
		}

		[Test]
		public void IsOneOfThreeParameters()
		{
			PropertyChangedEventArgs args = new("MyProperty");
			BindableProperty bindable0 = BindableProperty.Create("MyProperty", typeof(object), typeof(object));
			BindableProperty bindable1 = BindableProperty.Create("AlsoMyProperty", typeof(object), typeof(object));
			BindableProperty bindable2 = BindableProperty.Create("AlsoMyProperty1", typeof(object), typeof(object));

			Assert.IsTrue(args.IsOneOf(bindable0, bindable1, bindable2));
		}

		[Test]
		public void IsOneOfFourParameters()
		{
			PropertyChangedEventArgs args = new("MyProperty");
			BindableProperty bindable0 = BindableProperty.Create("MyProperty", typeof(object), typeof(object));
			BindableProperty bindable1 = BindableProperty.Create("AlsoMyProperty", typeof(object), typeof(object));
			BindableProperty bindable2 = BindableProperty.Create("AlsoMyProperty1", typeof(object), typeof(object));
			BindableProperty bindable3 = BindableProperty.Create("NotMyProperty", typeof(object), typeof(object));

			Assert.IsTrue(args.IsOneOf(bindable0, bindable1, bindable2, bindable3));
		}

		[Test]
		public void IsOneOfFiveParameters()
		{
			PropertyChangedEventArgs args = new("MyProperty");
			BindableProperty bindable0 = BindableProperty.Create("MyProperty", typeof(object), typeof(object));
			BindableProperty bindable1 = BindableProperty.Create("AlsoMyProperty", typeof(object), typeof(object));
			BindableProperty bindable2 = BindableProperty.Create("AlsoMyProperty1", typeof(object), typeof(object));
			BindableProperty bindable3 = BindableProperty.Create("NotMyProperty", typeof(object), typeof(object));
			BindableProperty bindable4 = BindableProperty.Create("AlsNotMyProperty", typeof(object), typeof(object));

			Assert.IsTrue(args.IsOneOf(bindable0, bindable1, bindable2, bindable3, bindable4));
		}

		[Test]
		[TestCase("")]
		[TestCase(null)]
		public void IsOneOfTwoParametersNullOrEmptyValue(string changedPropertyName)
		{
			PropertyChangedEventArgs args = new(changedPropertyName);
			BindableProperty bindable0 = BindableProperty.Create("MyProperty", typeof(object), typeof(object));
			BindableProperty bindable1 = BindableProperty.Create("AlsoMyProperty", typeof(object), typeof(object));

			Assert.IsTrue(args.IsOneOf(bindable0, bindable1));
		}

		[Test]
		[TestCase("")]
		[TestCase(null)]
		public void IsOneOfThreeParametersNullOrEmptyValue(string changedPropertyName)
		{
			PropertyChangedEventArgs args = new(changedPropertyName);
			BindableProperty bindable0 = BindableProperty.Create("MyProperty", typeof(object), typeof(object));
			BindableProperty bindable1 = BindableProperty.Create("AlsoMyProperty", typeof(object), typeof(object));
			BindableProperty bindable2 = BindableProperty.Create("AlsoMyProperty1", typeof(object), typeof(object));

			Assert.IsTrue(args.IsOneOf(bindable0, bindable1, bindable2));
		}

		[Test]
		[TestCase("")]
		[TestCase(null)]
		public void IsOneOfFourParametersNullOrEmptyValue(string changedPropertyName)
		{
			PropertyChangedEventArgs args = new(changedPropertyName);
			BindableProperty bindable0 = BindableProperty.Create("MyProperty", typeof(object), typeof(object));
			BindableProperty bindable1 = BindableProperty.Create("AlsoMyProperty", typeof(object), typeof(object));
			BindableProperty bindable2 = BindableProperty.Create("AlsoMyProperty1", typeof(object), typeof(object));
			BindableProperty bindable3 = BindableProperty.Create("NotMyProperty", typeof(object), typeof(object));

			Assert.IsTrue(args.IsOneOf(bindable0, bindable1, bindable2, bindable3));
		}

		[Test]
		[TestCase("")]
		[TestCase(null)]
		public void IsOneOfFiveParametersNullOrEmptyValue(string changedPropertyName)
		{
			PropertyChangedEventArgs args = new(changedPropertyName);
			BindableProperty bindable0 = BindableProperty.Create("MyProperty", typeof(object), typeof(object));
			BindableProperty bindable1 = BindableProperty.Create("AlsoMyProperty", typeof(object), typeof(object));
			BindableProperty bindable2 = BindableProperty.Create("AlsoMyProperty1", typeof(object), typeof(object));
			BindableProperty bindable3 = BindableProperty.Create("NotMyProperty", typeof(object), typeof(object));
			BindableProperty bindable4 = BindableProperty.Create("AlsNotMyProperty", typeof(object), typeof(object));

			Assert.IsTrue(args.IsOneOf(bindable0, bindable1, bindable2, bindable3, bindable4));
		}
	}
}
