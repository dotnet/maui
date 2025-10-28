using System;
using System.Globalization;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class Bz47703Converter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value != null)
				return "Label:" + value;
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value;
		}
	}

	public class Bz47703View : Label
	{
		BindingBase displayBinding;

		public BindingBase DisplayBinding
		{
			get { return displayBinding; }
			set
			{
				displayBinding = value;
				if (displayBinding != null)
					this.SetBinding(TextProperty, DisplayBinding);
			}
		}
	}

	public partial class Bz47703 : ContentPage
	{
		public Bz47703()
		{
			InitializeComponent();
		}

		public Bz47703(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{
			[InlineData(true)]
			[InlineData(false)]
			public void IValueConverterOnBindings(bool useCompiledXaml)
			{
				var page = new Bz47703(useCompiledXaml);
				page.BindingContext = new { Name = "Foo" };
				Assert.Equal("Label:Foo", page.view.Text);
			}
		}
	}
}
