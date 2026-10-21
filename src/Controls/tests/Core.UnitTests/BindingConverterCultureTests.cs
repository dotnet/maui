using System;
using System.Globalization;
using System.Threading;
using Microsoft.Maui.Controls.Internals;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class BindingConverterCultureTests : BaseTestFixture
	{
		[Fact]
		public void BindingConverterCultureOverridesCurrentUICulture()
		{
			var originalCulture = Thread.CurrentThread.CurrentUICulture;

			try
			{
				Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");

				var vm = new MockViewModel { Text = "Text" };
				var bindable = new MockBindable { BindingContext = vm };
				bindable.SetBinding(MockBindable.TextProperty, new Binding("Text", converter: new CultureNameConverter())
				{
					ConverterCulture = new CultureInfo("nl-NL")
				});

				Assert.Equal("nl-NL", bindable.Text);

				bindable.SetValueFromRenderer(MockBindable.TextProperty, "Updated");

				Assert.Equal("nl-NL", vm.Text);
			}
			finally
			{
				Thread.CurrentThread.CurrentUICulture = originalCulture;
			}
		}

		[Fact]
		public void TypedBindingConverterCultureOverridesCurrentUICulture()
		{
			var originalCulture = Thread.CurrentThread.CurrentUICulture;

			try
			{
				Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");

				var vm = new MockViewModel { Text = "Text" };
				var bindable = new MockBindable { BindingContext = vm };
				bindable.SetBinding(MockBindable.TextProperty, CreateTypedBinding(new CultureInfo("nl-NL")));

				Assert.Equal("nl-NL", bindable.Text);

				bindable.SetValueFromRenderer(MockBindable.TextProperty, "Updated");

				Assert.Equal("nl-NL", vm.Text);
			}
			finally
			{
				Thread.CurrentThread.CurrentUICulture = originalCulture;
			}
		}

		[Fact]
		public void ClonesPreserveExplicitConverterCulture()
		{
			var culture = new CultureInfo("nl-NL");

			var bindingClone = (Binding)new Binding(".", converter: new CultureNameConverter())
			{
				ConverterCulture = culture
			}.Clone();

			var typedBindingClone = (TypedBinding<MockViewModel, string>)CreateTypedBinding(culture).Clone();

			Assert.Same(culture, bindingClone.ConverterCulture);
			Assert.Same(culture, typedBindingClone.ConverterCulture);
		}

		[Fact]
		public void ClonesKeepUnsetConverterCultureDynamic()
		{
			var originalCulture = Thread.CurrentThread.CurrentUICulture;

			try
			{
				Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");

				var bindingClone = (Binding)new Binding(".", converter: new CultureNameConverter()).Clone();
				var typedBindingClone = (TypedBinding<MockViewModel, string>)CreateTypedBinding().Clone();

				Thread.CurrentThread.CurrentUICulture = new CultureInfo("nl-NL");

				Assert.Equal("nl-NL", bindingClone.ConverterCulture.Name);
				Assert.Equal("nl-NL", typedBindingClone.ConverterCulture.Name);
			}
			finally
			{
				Thread.CurrentThread.CurrentUICulture = originalCulture;
			}
		}

		static TypedBinding<MockViewModel, string> CreateTypedBinding(CultureInfo culture = null)
		{
			var binding = new TypedBinding<MockViewModel, string>(
				getter: vm => (vm.Text, true),
				setter: (vm, value) => vm.Text = value,
				handlers: new[]
				{
					new Tuple<Func<MockViewModel, object>, string>(vm => vm, nameof(MockViewModel.Text))
				})
			{
				Converter = new CultureNameConverter()
			};

			if (culture is not null)
				binding.ConverterCulture = culture;

			return binding;
		}

		class CultureNameConverter : IValueConverter
		{
			public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => culture.Name;

			public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => culture.Name;
		}
	}
}
