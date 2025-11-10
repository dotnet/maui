using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[AcceptEmptyServiceProvider]
	public class Bz34037Converter0 : IValueConverter
	{
		public static int Invoked { get; set; }
		public static object Parameter { get; set; }
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			Invoked++;
			Parameter = parameter;
			return true;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		public object ProvideValue(IServiceProvider serviceProvider)
		{
			return new Bz34037Converter0();
		}
	}

	[AcceptEmptyServiceProvider]
	public class Bz34037Converter1 : IValueConverter, IMarkupExtension
	{
		public static int Invoked { get; set; }
		public static object Parameter { get; set; }
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			Invoked++;
			Parameter = parameter;
			return true;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		public object ProvideValue(IServiceProvider serviceProvider)
		{
			return new Bz34037Converter1();
		}
	}

	public partial class Bz34037 : ContentPage
	{
		public Bz34037()
		{
			InitializeComponent();
		}

		public string Property
		{
			get { return "FooBar"; }
		}


		public class Tests : IDisposable
		{
			public void Dispose()
			{
				DispatcherProvider.SetCurrent(null);
				Application.SetCurrentApplication(null);
			}
			public Tests()
			{
				Application.SetCurrentApplication(new MockApplication());
				DispatcherProvider.SetCurrent(new DispatcherProviderStub());
				Bz34037Converter0.Invoked = 0;
				Bz34037Converter1.Invoked = 0;
			}


			[Theory]
			[Values]
			public void ConverterParameterOrderDoesNotMatters(XamlInflator inflator)
			{
				var layout = new Bz34037(inflator);
				Assert.Equal(1, Bz34037Converter0.Invoked);
				Assert.Equal(1, Bz34037Converter1.Invoked);
				Assert.Equal(typeof(string), Bz34037Converter0.Parameter);
				Assert.Equal(typeof(string), Bz34037Converter1.Parameter);
				Assert.True(layout.s0.IsToggled);
				Assert.True(layout.s1.IsToggled);
			}
		}
	}
}