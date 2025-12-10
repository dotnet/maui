using System;
using System.Globalization;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui32837 : Application
{
	public Maui32837() => InitializeComponent();

	class Test
	{
		[SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		[TearDown]
		public void TearDown()
		{
			AppInfo.SetCurrent(null);
			Application.SetCurrentApplication(null);
		}

		[Test]
		public void ConverterReceivesCorrectValueFromStaticResource([Values] XamlInflator inflator)
		{
			var app = new Maui32837(inflator);
			
			// Get the converter from resources
			var converter = app.Resources["IntToCornerRadiusConverter"] as Maui32837IntToCornerRadiusConverter;
			Assert.IsNotNull(converter, "Converter should not be null");
			
			// Get the RoundRectangle from resources
			var roundRect = app.Resources["MyRoundRectangle"] as RoundRectangle;
			Assert.IsNotNull(roundRect, "RoundRectangle should not be null");
			
			// The binding should have been evaluated and converter should have been called
			// Check that the converter was actually invoked by looking at the result
			var cornerRadius = roundRect.CornerRadius;
			
			// The converter should have converted the int value 16 to CornerRadius(16)
			Assert.That(cornerRadius.TopLeft, Is.EqualTo(16.0), 
				$"TopLeft corner radius should be 16.0 for {inflator}, but was {cornerRadius.TopLeft}");
			Assert.That(cornerRadius.TopRight, Is.EqualTo(16.0), 
				$"TopRight corner radius should be 16.0 for {inflator}, but was {cornerRadius.TopRight}");
			Assert.That(cornerRadius.BottomLeft, Is.EqualTo(16.0), 
				$"BottomLeft corner radius should be 16.0 for {inflator}, but was {cornerRadius.BottomLeft}");
			Assert.That(cornerRadius.BottomRight, Is.EqualTo(16.0), 
				$"BottomRight corner radius should be 16.0 for {inflator}, but was {cornerRadius.BottomRight}");
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
