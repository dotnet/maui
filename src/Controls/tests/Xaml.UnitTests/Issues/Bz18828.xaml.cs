using System;
using System.Globalization;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class Bz18828Converter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is double width)
			{
				int result = (int)Math.Floor(width / 160d);
				return Math.Max(1, result);
			}
			else
			{
				return 1;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public partial class Bz18828 : ContentPage
	{
		public Bz18828()
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
			public void GridItemsLayoutWithConverter(XamlInflator inflator)
			{
				var layout = new Bz18828(inflator);
				Assert.True(((GridItemsLayout)layout.collectionView.ItemsLayout).Span == 2);
			}
		}
	}
}