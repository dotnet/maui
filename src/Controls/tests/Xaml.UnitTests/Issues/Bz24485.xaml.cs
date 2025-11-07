using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class Bz24485Converter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null)
				return null;

			return new
			{
				Date = DateTime.Now.ToString("dd MMMM yyyy"),
				Object = new object()
			};
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public partial class Bz24485 : ContentPage
	{
		public Bz24485()
		{
			InitializeComponent();
		}


		public class Tests
		{
			[Theory]
			[Values]
			public void BindingContextWithConverter(XamlInflator inflator)
			{
				var layout = new Bz24485(inflator);
				layout.BindingContext = new { Data1 = new object() };
				// TODO: XUnit has no // TODO: XUnit has no Assert.Pass() - test passes if no exception is thrown - test passes if no exception is thrown
			}
		}
	}
}