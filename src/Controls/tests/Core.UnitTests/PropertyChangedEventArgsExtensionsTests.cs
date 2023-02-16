using System.ComponentModel;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class PropertyChangedEventArgsExtensionsTests : BaseTestFixture
	{
		[Fact]
		public void IsWithMathingStringValues()
		{
			PropertyChangedEventArgs args = new("MyProperty");
			BindableProperty bindable = BindableProperty.Create("MyProperty", typeof(object), typeof(object));

			Assert.True(args.Is(bindable));
		}

		[Fact]
		public void IsWithNonMathingValues()
		{
			PropertyChangedEventArgs args = new("MyProperty");
			BindableProperty bindable = BindableProperty.Create("NotMyProperty", typeof(object), typeof(object));

			Assert.False(args.Is(bindable));
		}

		[Theory]
		[InlineData("")]
		[InlineData(null)]
		public void IsWithNullOrEmptyValue(string changedPropertyName)
		{
			PropertyChangedEventArgs args = new(changedPropertyName);
			BindableProperty bindable = BindableProperty.Create("MyProperty", typeof(object), typeof(object));

			Assert.True(args.Is(bindable));
		}

		[Fact]
		public void IsOneOfTwoParameters()
		{
			PropertyChangedEventArgs args = new("MyProperty");
			BindableProperty bindable0 = BindableProperty.Create("MyProperty", typeof(object), typeof(object));
			BindableProperty bindable1 = BindableProperty.Create("AlsoMyProperty", typeof(object), typeof(object));

			Assert.True(args.IsOneOf(bindable0, bindable1));
		}

		[Fact]
		public void IsOneOfThreeParameters()
		{
			PropertyChangedEventArgs args = new("MyProperty");
			BindableProperty bindable0 = BindableProperty.Create("MyProperty", typeof(object), typeof(object));
			BindableProperty bindable1 = BindableProperty.Create("AlsoMyProperty", typeof(object), typeof(object));
			BindableProperty bindable2 = BindableProperty.Create("AlsoMyProperty1", typeof(object), typeof(object));

			Assert.True(args.IsOneOf(bindable0, bindable1, bindable2));
		}

		[Fact]
		public void IsOneOfFourParameters()
		{
			PropertyChangedEventArgs args = new("MyProperty");
			BindableProperty bindable0 = BindableProperty.Create("MyProperty", typeof(object), typeof(object));
			BindableProperty bindable1 = BindableProperty.Create("AlsoMyProperty", typeof(object), typeof(object));
			BindableProperty bindable2 = BindableProperty.Create("AlsoMyProperty1", typeof(object), typeof(object));
			BindableProperty bindable3 = BindableProperty.Create("NotMyProperty", typeof(object), typeof(object));

			Assert.True(args.IsOneOf(bindable0, bindable1, bindable2, bindable3));
		}

		[Fact]
		public void IsOneOfFiveParameters()
		{
			PropertyChangedEventArgs args = new("MyProperty");
			BindableProperty bindable0 = BindableProperty.Create("MyProperty", typeof(object), typeof(object));
			BindableProperty bindable1 = BindableProperty.Create("AlsoMyProperty", typeof(object), typeof(object));
			BindableProperty bindable2 = BindableProperty.Create("AlsoMyProperty1", typeof(object), typeof(object));
			BindableProperty bindable3 = BindableProperty.Create("NotMyProperty", typeof(object), typeof(object));
			BindableProperty bindable4 = BindableProperty.Create("AlsNotMyProperty", typeof(object), typeof(object));

			Assert.True(args.IsOneOf(bindable0, bindable1, bindable2, bindable3, bindable4));
		}

		[Theory]
		[InlineData("")]
		[InlineData(null)]
		public void IsOneOfTwoParametersNullOrEmptyValue(string changedPropertyName)
		{
			PropertyChangedEventArgs args = new(changedPropertyName);
			BindableProperty bindable0 = BindableProperty.Create("MyProperty", typeof(object), typeof(object));
			BindableProperty bindable1 = BindableProperty.Create("AlsoMyProperty", typeof(object), typeof(object));

			Assert.True(args.IsOneOf(bindable0, bindable1));
		}

		[Theory]
		[InlineData("")]
		[InlineData(null)]
		public void IsOneOfThreeParametersNullOrEmptyValue(string changedPropertyName)
		{
			PropertyChangedEventArgs args = new(changedPropertyName);
			BindableProperty bindable0 = BindableProperty.Create("MyProperty", typeof(object), typeof(object));
			BindableProperty bindable1 = BindableProperty.Create("AlsoMyProperty", typeof(object), typeof(object));
			BindableProperty bindable2 = BindableProperty.Create("AlsoMyProperty1", typeof(object), typeof(object));

			Assert.True(args.IsOneOf(bindable0, bindable1, bindable2));
		}

		[Theory]
		[InlineData("")]
		[InlineData(null)]
		public void IsOneOfFourParametersNullOrEmptyValue(string changedPropertyName)
		{
			PropertyChangedEventArgs args = new(changedPropertyName);
			BindableProperty bindable0 = BindableProperty.Create("MyProperty", typeof(object), typeof(object));
			BindableProperty bindable1 = BindableProperty.Create("AlsoMyProperty", typeof(object), typeof(object));
			BindableProperty bindable2 = BindableProperty.Create("AlsoMyProperty1", typeof(object), typeof(object));
			BindableProperty bindable3 = BindableProperty.Create("NotMyProperty", typeof(object), typeof(object));

			Assert.True(args.IsOneOf(bindable0, bindable1, bindable2, bindable3));
		}

		[Theory]
		[InlineData("")]
		[InlineData(null)]
		public void IsOneOfFiveParametersNullOrEmptyValue(string changedPropertyName)
		{
			PropertyChangedEventArgs args = new(changedPropertyName);
			BindableProperty bindable0 = BindableProperty.Create("MyProperty", typeof(object), typeof(object));
			BindableProperty bindable1 = BindableProperty.Create("AlsoMyProperty", typeof(object), typeof(object));
			BindableProperty bindable2 = BindableProperty.Create("AlsoMyProperty1", typeof(object), typeof(object));
			BindableProperty bindable3 = BindableProperty.Create("NotMyProperty", typeof(object), typeof(object));
			BindableProperty bindable4 = BindableProperty.Create("AlsNotMyProperty", typeof(object), typeof(object));

			Assert.True(args.IsOneOf(bindable0, bindable1, bindable2, bindable3, bindable4));
		}
	}
}
