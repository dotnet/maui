using System;
using System.Globalization;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui25309 : ContentPage
{
	public Maui25309() => InitializeComponent();

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
			Application.Current = null;
			DispatcherProvider.SetCurrent(null);
		}

		[Theory]
		[Values]
		public void GenericConvertersDoesNotThrowNRE(XamlInflator inflator)
		{
			var page = new Maui25309(inflator) { BindingContext = new { IsValid = true } };
			var converter = page.Resources["IsValidConverter"] as Maui25309BoolToObjectConverter;
			Assert.NotNull(converter);
			Assert.Equal(Color.Parse("#140F4B"), converter.TrueObject); // TODO: Was Parse with 2 params, added actual value
		}
	}
}

#nullable enable
class Maui25309BoolToObjectConverter : Maui25309BoolToObjectConverter<object>
{
}

public class Maui25309BoolToObjectConverter<TObject> : IValueConverter
{
	public TObject? TrueObject { get; set; }

	public TObject? FalseObject { get; set; }

	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		bool boolValue = false;
		if (value is bool bv)
			boolValue = bv;

		return boolValue ? TrueObject : FalseObject;
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
