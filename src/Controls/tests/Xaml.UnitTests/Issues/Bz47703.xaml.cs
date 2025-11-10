using System;
using System.Globalization;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
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


	public class Tests : IDisposable
	{
		public Tests()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose()
		{
			AppInfo.SetCurrent(null);
			DispatcherProvider.SetCurrent(null);
			Application.SetCurrentApplication(null);
		}
		[Theory]
		[Values]
		public void IValueConverterOnBindings(XamlInflator inflator)
		{
			var page = new Bz47703(inflator);
			page.BindingContext = new { Name = "Foo" };
			Assert.Equal("Label:Foo", page.view.Text);
		}
	}
}
