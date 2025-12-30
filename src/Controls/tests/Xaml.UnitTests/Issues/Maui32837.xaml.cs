using System;
using System.Globalization;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui32837 : Application
{
	public Maui32837() => InitializeComponent();

	[Collection("Issue")]
	public class Test : IDisposable
	{
		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose()
		{
			AppInfo.SetCurrent(null);
			Application.SetCurrentApplication(null);
		}

		[Theory]
		[XamlInflatorData]
		internal void ConverterReceivesCorrectValueFromStaticResource(XamlInflator inflator)
		{
			var app = new Maui32837(inflator);
			
			// Get the converter from resources
			var converter = app.Resources["IntToCornerRadiusConverter"] as Maui32837IntToCornerRadiusConverter;
			Assert.NotNull(converter);
			
			// Get the RoundRectangle from resources
			var roundRect = app.Resources["MyRoundRectangle"] as RoundRectangle;
			Assert.NotNull(roundRect);
			
			// The binding should have been evaluated and converter should have been called
			// Check that the converter was actually invoked by looking at the result
			var cornerRadius = roundRect.CornerRadius;
			
			// The converter should have converted the int value 16 to CornerRadius(16)
			Assert.Equal(16.0, cornerRadius.TopLeft);
			Assert.Equal(16.0, cornerRadius.TopRight);
			Assert.Equal(16.0, cornerRadius.BottomLeft);
			Assert.Equal(16.0, cornerRadius.BottomRight);
		}
	}
}

#nullable enable
public class Maui32837IntToCornerRadiusConverter : IValueConverter
{
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is int radius)
		{
			return new CornerRadius(radius);
		}
		return new CornerRadius(0);
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is CornerRadius cornerRadius)
		{
			return (int)cornerRadius.TopLeft;
		}
		return 0;
	}
}
