using System;
using System.Globalization;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	using StackLayout = Microsoft.Maui.Controls.Compatibility.StackLayout;

	// https://github.com/dotnet/maui/issues/10792
	public class MultiBindingOnBindingContextTests : BaseTestFixture
	{
		class Source
		{
			public string Property1 { get; set; }
			public string Property2 { get; set; }
		}

		class JoinConverter : IMultiValueConverter
		{
			public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
				=> string.Join("|", values);

			public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
				=> throw new NotSupportedException();
		}

		static MultiBinding CreateMultiBinding(BindingMode childMode = BindingMode.Default) => new MultiBinding
		{
			Converter = new JoinConverter(),
			Bindings =
			{
				new Binding(nameof(Source.Property1), childMode),
				new Binding(nameof(Source.Property2), childMode),
			}
		};

		[Theory]
		[InlineData(BindingMode.Default)]
		[InlineData(BindingMode.OneTime)]
		public void MultiBindingOnBindingContextUsesInheritedContextSetLater(BindingMode childMode)
		{
			var label = new Label();
			label.SetBinding(Label.TextProperty, new Binding("."));
			label.SetBinding(BindableObject.BindingContextProperty, CreateMultiBinding(childMode));

			var parent = new StackLayout { Children = { label } };
			parent.BindingContext = new Source { Property1 = "a", Property2 = "b" };

			Assert.Equal("a|b", label.BindingContext);
			Assert.Equal("a|b", label.Text);
		}

		[Fact]
		public void MultiBindingOnBindingContextUsesInheritedContextSetBefore()
		{
			var label = new Label();
			var parent = new StackLayout { Children = { label } };
			parent.BindingContext = new Source { Property1 = "a", Property2 = "b" };

			label.SetBinding(BindableObject.BindingContextProperty, CreateMultiBinding());

			Assert.Equal("a|b", label.BindingContext);
		}

		[Fact]
		public void MultiBindingOnBindingContextUpdatesWhenInheritedContextChanges()
		{
			var label = new Label();
			label.SetBinding(BindableObject.BindingContextProperty, CreateMultiBinding());
			var parent = new StackLayout { Children = { label } };

			parent.BindingContext = new Source { Property1 = "a", Property2 = "b" };
			Assert.Equal("a|b", label.BindingContext);

			parent.BindingContext = new Source { Property1 = "c", Property2 = "d" };
			Assert.Equal("c|d", label.BindingContext);
		}

		[Fact]
		public void MultiBindingOnBindingContextFollowsSourceAndInheritedContextChanges()
		{
			var label = new Label();
			label.SetBinding(BindableObject.BindingContextProperty, new MultiBinding
			{
				Converter = new JoinConverter(),
				Bindings = { new Binding(nameof(MockViewModel.Text)), new Binding(nameof(MockViewModel.Text)) }
			});
			var parent = new StackLayout { Children = { label } };

			var vm = new MockViewModel("a");
			parent.BindingContext = vm;
			Assert.Equal("a|a", label.BindingContext);

			vm.Text = "b";
			Assert.Equal("b|b", label.BindingContext);

			parent.BindingContext = new MockViewModel("c");
			Assert.Equal("c|c", label.BindingContext);
		}
	}
}
