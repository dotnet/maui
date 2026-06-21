using System;
using System.Globalization;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

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

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void IValueConverterOnBindings(XamlInflator inflator)
		{
			var page = new Bz47703(inflator);
			page.BindingContext = new { Name = "Foo" };
			Assert.Equal("Label:Foo", page.view.Text);
		}
	}
}
