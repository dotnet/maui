using System;
using System.Globalization;
using NUnit.Framework;

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

		public Bz18828(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[TestCase(true)]
			[TestCase(false)]
			public void GridItemsLayoutWithConverter(bool useCompiledXaml)
			{
				var layout = new Bz18828(useCompiledXaml);
				Assert.True(((GridItemsLayout)layout.collectionView.ItemsLayout).Span == 2);
			}
		}
	}
}