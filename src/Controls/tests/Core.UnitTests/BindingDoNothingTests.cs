using System;
using System.Globalization;
using Microsoft.Maui.Controls.Internals;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	// https://github.com/dotnet/maui/issues/11740
	public class BindingDoNothingTests : BaseTestFixture
	{
		const string Skip = "skip";

		class DoNothingConverter : IValueConverter
		{
			public bool DoNothingOnConvert { get; set; } = true;
			public bool DoNothingOnConvertBack { get; set; } = true;

			// Returns Binding.DoNothing for the "skip" value, passes everything else through
			public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
				=> DoNothingOnConvert && Equals(value, Skip) ? Binding.DoNothing : value;

			public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
				=> DoNothingOnConvertBack ? Binding.DoNothing : value;
		}

		static TypedBinding<MockViewModel, string> CreateTypedBinding(IValueConverter converter, BindingMode mode)
			=> new TypedBinding<MockViewModel, string>(
				getter: mvm => (mvm.Text, true),
				setter: (mvm, s) => mvm.Text = s,
				handlers: new[] { new Tuple<Func<MockViewModel, object>, string>(mvm => mvm, "Text") })
			{
				Converter = converter,
				Mode = mode,
			};

		[Fact]
		public void DoNothingFromConvertDoesNotUpdateTarget()
		{
			var vm = new MockViewModel("first");
			var label = new Label { BindingContext = vm };
			label.SetBinding(Label.TextProperty, new Binding("Text", converter: new DoNothingConverter()));
			Assert.Equal("first", label.Text);

			vm.Text = Skip;
			Assert.Equal("first", label.Text);

			vm.Text = "second";
			Assert.Equal("second", label.Text);
		}

		[Fact]
		public void DoNothingFromConvertOnInitialApplyKeepsTargetValue()
		{
			var label = new Label { Text = "initial", BindingContext = new MockViewModel(Skip) };
			label.SetBinding(Label.TextProperty, new Binding("Text", converter: new DoNothingConverter()));

			Assert.Equal("initial", label.Text);
		}

		[Fact]
		public void DoNothingFromConvertDoesNotApplyTargetNullValueOrStringFormat()
		{
			var vm = new MockViewModel("first");
			var label = new Label { BindingContext = vm };
			label.SetBinding(Label.TextProperty, new Binding("Text", converter: new DoNothingConverter(), stringFormat: "fmt {0}") { TargetNullValue = "null" });
			Assert.Equal("fmt first", label.Text);

			vm.Text = Skip;
			Assert.Equal("fmt first", label.Text);
		}

		[Fact]
		public void DoNothingFromConvertBackDoesNotUpdateSource()
		{
			var vm = new MockViewModel("source");
			var entry = new Entry();
			entry.SetBinding(Entry.TextProperty, new Binding("Text", BindingMode.TwoWay, new DoNothingConverter { DoNothingOnConvert = false }));
			entry.BindingContext = vm;
			Assert.Equal("source", entry.Text);

			entry.Text = "changed";

			Assert.Equal("source", vm.Text);
		}

		[Fact]
		public void TypedBindingDoNothingFromConvertDoesNotUpdateTarget()
		{
			var vm = new MockViewModel("first");
			var label = new Label { BindingContext = vm };
			label.SetBinding(Label.TextProperty, CreateTypedBinding(new DoNothingConverter(), BindingMode.OneWay));
			Assert.Equal("first", label.Text);

			vm.Text = Skip;
			Assert.Equal("first", label.Text);

			vm.Text = "second";
			Assert.Equal("second", label.Text);
		}

		[Fact]
		public void TypedBindingDoNothingFromConvertBackDoesNotUpdateSource()
		{
			var vm = new MockViewModel("source");
			var entry = new Entry();
			entry.SetBinding(Entry.TextProperty, CreateTypedBinding(new DoNothingConverter { DoNothingOnConvert = false }, BindingMode.TwoWay));
			entry.BindingContext = vm;
			Assert.Equal("source", entry.Text);

			entry.Text = "changed";

			Assert.Equal("source", vm.Text);
		}
	}
}
