using System;
using System.Globalization;
using System.Threading;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui5696 : ContentPage
{
	public Maui5696()
	{
		InitializeComponent();
	}

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void BindingConverterCultureOverridesCurrentUICulture(XamlInflator inflator)
		{
			var originalCulture = Thread.CurrentThread.CurrentUICulture;

			try
			{
				Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");

				var page = new Maui5696(inflator)
				{
					BindingContext = new Maui5696ViewModel { Text = "Text" }
				};

				Assert.Equal("nl-NL", page.label.Text);
			}
			finally
			{
				Thread.CurrentThread.CurrentUICulture = originalCulture;
			}
		}
	}
}

public class Maui5696ViewModel
{
	public string Text { get; set; } = string.Empty;
}

public class Maui5696Converter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		=> culture.Name;

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		=> culture.Name;
}
